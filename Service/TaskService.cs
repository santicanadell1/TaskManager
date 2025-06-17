using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Domain.Exceptions.TaskExceptions;
using Service.Converter;
using Service.Exceptions.ResourceServiceExceptions;
using Service.Interface;
using Service.Models;
using Task = Domain.Task;

namespace Service;

public class TaskService : ITaskService
{
    private readonly CpmService _cpmService;
    private readonly IRepositoryManager _repositoryManager;
    private readonly ResourceConverter _resourceConverter;
    private readonly ResourceService _resourceService;
    private readonly TaskConverter _taskConverter;

    public TaskService(IRepositoryManager repositoryManager, CpmService cpmService)
    {
        _repositoryManager = repositoryManager;
        _taskConverter = new TaskConverter(_repositoryManager);
        _resourceConverter = new ResourceConverter(_repositoryManager);
        _resourceService = new ResourceService(_repositoryManager);
        _cpmService = cpmService;
    }

    public void UpdateTask(TaskDTO taskDTO)
    {
        var task = _taskConverter.ToEntity(taskDTO);
        _repositoryManager.TaskRepository.Update(task);
    }

    public void DeleteTask(TaskDTO taskDTO)
    {
        var task = _taskConverter.ToEntity(taskDTO);
        _repositoryManager.TaskRepository.Delete(task);
    }

    public TaskDTO GetTask(string title)
    {
        return _taskConverter.FromEntity(_repositoryManager.TaskRepository.Get(t => t.Title == title));
    }

    public List<TaskDTO> GetTasks()
    {
        var tasks = new List<TaskDTO>();
        foreach (var task in _repositoryManager.TaskRepository.GetAll()) tasks.Add(_taskConverter.FromEntity(task));

        return tasks;
    }

    public void AddTask(string projectName, TaskDTO taskDTO, bool solve = false)
    {
        var notificationService = new NotificationService(_repositoryManager);
        var _projectService = new AdminPService(_repositoryManager);
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();
        if (taskDTO.ExpectedStartDate.AddDays(1) <= project.StartDate)
            throw new TaskException("The task's start date is before the project's start date.");

        var startDate =
            GetNextDateAvailable(solve, taskDTO.ExpectedStartDate, taskDTO.Duration, taskDTO.Resources, taskDTO.Title);
        taskDTO.ExpectedStartDate = startDate;
        taskDTO = _resourceService.updateResourceDependencies(taskDTO, projectName);
        CreateTask(taskDTO);
        var task = _repositoryManager.TaskRepository.Get(t => t.Title == taskDTO.Title);

        _repositoryManager.ProjectRepository.AddTask(projectName, task);

        RecalculateCriticalPath(projectName);
        var cpmResult = GetCriticalPath(projectName);
        if (cpmResult.CriticalTaskIds.Contains(task.Id))
        {
            var notificationDTO = new NotificationDTO
            {
                Read = false,
                Description = $"The task {task.Title} has been created. The critical path has changed.",
                Project = _projectService.GetProjectByName(projectName)
            };
            notificationService.CreateNotification(notificationDTO);
        }
    }

    public void DeleteTask(string projectName, string title)
    {
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        var task = project.Tasks.FirstOrDefault(t => t.Title == title);
        if (task == null) throw new TaskNotFoundException();

        _repositoryManager.ProjectRepository.RemoveTask(projectName, task.Id);
        var taskEntity = _repositoryManager.TaskRepository.Get(t => t.Title == title);
        _repositoryManager.TaskRepository.Delete(taskEntity);

        RecalculateCriticalPath(projectName);
    }

    public void UpdateTask(string projectName, string title, TaskDTO taskDTO, bool solve = false)
    {
        var notificationService = new NotificationService(_repositoryManager);
        var projectService = new AdminPService(_repositoryManager);
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();
        var original = project.Tasks.FirstOrDefault(t => t.Title == title);
        if (original == null) throw new TaskNotFoundException();

        var startDate = GetNextDateAvailable(
            solve,
            taskDTO.ExpectedStartDate,
            taskDTO.Duration,
            taskDTO.Resources,
            taskDTO.Title
        );
        taskDTO.ExpectedStartDate = startDate;
        taskDTO = _resourceService.updateResourceDependencies(taskDTO, projectName);
        var previousEntities = new List<Task>();
        if (taskDTO.PreviousTasks != null)
            foreach (var prevDto in taskDTO.PreviousTasks)
                if (prevDto.Id.HasValue)
                {
                    var ent = project.Tasks.FirstOrDefault(t => t.Id == prevDto.Id.Value);
                    if (ent != null && ent.Title != title)
                        previousEntities.Add(ent);
                }

        var sameTimeEntities = new List<Task>();
        if (taskDTO.SameTimeTasks != null)
            foreach (var sameDto in taskDTO.SameTimeTasks)
                if (sameDto.Id.HasValue)
                {
                    var ent = project.Tasks.FirstOrDefault(t => t.Id == sameDto.Id.Value);
                    if (ent != null && ent.Title != title)
                        sameTimeEntities.Add(ent);
                }

        var updatedTask = new Task(
            taskDTO.Title,
            taskDTO.Description,
            taskDTO.ExpectedStartDate,
            taskDTO.Duration,
            previousEntities,
            sameTimeEntities,
            _resourceConverter.ToResourceEntityList(taskDTO.Resources)
        );
        updatedTask.Id = original.Id;
        updatedTask.State = (State)taskDTO.State;
        _repositoryManager.TaskRepository.Update(updatedTask);
        _repositoryManager.ProjectRepository
            .UpdateTask(projectName, updatedTask.Id, updatedTask);
        RecalculateCriticalPath(projectName);
        var cpmResult = GetCriticalPath(projectName);
        if (cpmResult.CriticalTaskIds.Contains(updatedTask.Id))
        {
            var notificationDTO = new NotificationDTO
            {
                Read = false,
                Description = $"The task {updatedTask.Title} has been updated. The critical path has changed.",
                Project = projectService.GetProjectByName(projectName)
            };
            notificationService.CreateNotification(notificationDTO);
        }
    }


    public List<TaskDTO> GetTasks(string projectName)
    {
        try
        {
            if (string.IsNullOrEmpty(projectName)) throw new ArgumentException("Project name cannot be null or empty");
            var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
            if (project == null) throw new ProjectNotFoundException();
            if (project.Tasks == null || !project.Tasks.Any()) return new List<TaskDTO>();
            var taskDTOs = project.Tasks
                .Where(t => t.Id.HasValue)
                .Select(t => new TaskDTO
                {
                    Title = t.Title ?? string.Empty,
                    Description = t.Description ?? string.Empty,
                    ExpectedStartDate = t.ExpectedStartDate,
                    Duration = t.Duration,
                    State = (StateDTO)t.State,
                    Resources = _resourceConverter.FromResourceEntityList(t.Resources ?? new List<Resource>()),
                    Id = t.Id,
                    IsCritical = t.IsCritical,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    LatestStart = t.LatestStart,
                    LatestFinish = t.LatestFinish,
                    Slack = t.Slack,
                    PreviousTasks = new List<TaskDTO>(),
                    SameTimeTasks = new List<TaskDTO>()
                }).ToList();
            if (!taskDTOs.Any()) return new List<TaskDTO>();
            var taskDict = taskDTOs
                .Where(t => t.Id.HasValue)
                .ToDictionary(t => t.Id.Value, t => t);
            foreach (var task in project.Tasks.Where(t => t.Id.HasValue))
            {
                if (!taskDict.ContainsKey(task.Id.Value)) continue;

                var taskDto = taskDict[task.Id.Value];
                if (task.PreviousTasks != null)
                    foreach (var prevTask in task.PreviousTasks)
                        if (prevTask?.Id.HasValue == true && taskDict.ContainsKey(prevTask.Id.Value))
                            taskDto.PreviousTasks.Add(taskDict[prevTask.Id.Value]);

                if (task.SameTimeTasks != null)
                    foreach (var sameTask in task.SameTimeTasks)
                        if (sameTask?.Id.HasValue == true && taskDict.ContainsKey(sameTask.Id.Value))
                            taskDto.SameTimeTasks.Add(taskDict[sameTask.Id.Value]);
            }

            return taskDTOs;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTasks for project '{projectName}': {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public TaskDTO GetTask(string projectName, string title)
    {
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        var task = project.Tasks.FirstOrDefault(t => t.Title == title);
        if (task == null) throw new TaskNotFoundException();

        return _taskConverter.FromEntity(task);
    }

    public CpmResultDTO GetCriticalPath(string projectName)
    {
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        try
        {
            var cpmResult = _cpmService.CalculateCriticalPath(GetTasks(projectName));

            return new CpmResultDTO
            {
                ProjectDuration = cpmResult.ProjectDuration,
                CriticalTaskIds = cpmResult.CriticalTasks.Select(t => t.Id).ToList(),
                CriticalPathIds = cpmResult.CriticalPath.Select(t => t.Id).ToList(),
                EarliestStartDate = project.Tasks.Any() ? project.Tasks.Min(t => t.StartDate) : DateTime.Now,
                LatestFinishDate = project.Tasks.Any() ? project.Tasks.Max(t => t.EndDate) : DateTime.Now
            };
        }
        catch (Exception)
        {
            return new CpmResultDTO
            {
                ProjectDuration = 0,
                CriticalTaskIds = new List<int?>(),
                CriticalPathIds = new List<int?>(),
                EarliestStartDate = DateTime.Now,
                LatestFinishDate = DateTime.Now
            };
        }
    }


    private void CreateTask(TaskDTO taskDTO)
    {
        var task = _taskConverter.ToEntity(taskDTO);
        _repositoryManager.TaskRepository.Add(task);
    }

    private DateTime GetNextDateAvailable(bool solve, DateTime startDate, int duration, List<ResourceDTO> resources,
        string taskTitle = "")
    {
        var startDateNext = startDate;
        foreach (var res in resources)
        {
            if (!_resourceService.IsAvailable(res, startDate, duration, taskTitle) && !solve)
                throw new ResourceNotAvailableException();
            var next = _resourceService.NextDateAvailable(res, startDate, duration, taskTitle);
            if (next > startDate)
                startDateNext = next;
        }

        return startDateNext;
    }

    private void RecalculateCriticalPath(string projectName)
    {
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null || project.Tasks.Count == 0) return;

        try
        {
            var updatedTasks = _cpmService.CalculateCriticalPath(GetTasks(projectName)).AllTasks;

            foreach (var updatedDTO in updatedTasks)
            {
                var originalTask = project.Tasks.FirstOrDefault(t => t.Id == updatedDTO.Id);
                if (originalTask != null)
                {
                    originalTask.IsCritical = updatedDTO.IsCritical;
                    originalTask.StartDate = updatedDTO.StartDate;
                    originalTask.EndDate = updatedDTO.EndDate;
                    originalTask.LatestStart = updatedDTO.LatestStart;
                    originalTask.LatestFinish = updatedDTO.LatestFinish;
                    originalTask.Slack = updatedDTO.Slack;
                }
            }
        }
        catch (Exception)
        {
        }
    }

    private List<TaskDTO> ToMinimalTaskDTOList(List<Task> tasks)
    {
        if (tasks == null) return new List<TaskDTO>();

        return tasks.Select(t => new TaskDTO
        {
            Id = t.Id,
            Title = t.Title
        }).ToList();
    }
}