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
    private readonly IResourceService _resourceService;
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
        var entity = _repositoryManager.TaskRepository.Get(t => t.Title == title);
        return _taskConverter.FromEntity(entity);
    }

    public List<TaskDTO> GetTasks()
    {
        List<TaskDTO> tasks = new List<TaskDTO>();
        foreach (var t in _repositoryManager.TaskRepository.GetAll()) tasks.Add(_taskConverter.FromEntity(t));
        return tasks;
    }

    public void AddTask(string projectName, TaskDTO taskDTO, bool solve = false)
    {
        INotificationService notificationService = new NotificationService(_repositoryManager);
        IAdminPService projectService = new AdminPService(_repositoryManager);
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();
        if (taskDTO.ExpectedStartDate.AddDays(1) <= project.StartDate)
            throw new TaskException("The task's start date is before the project's start date.");

        var startDate = GetNextDateAvailable(
            solve,
            taskDTO.ExpectedStartDate,
            taskDTO.Duration,
            taskDTO.Resources,
            taskDTO.Title
        );
        taskDTO.ExpectedStartDate = startDate;
        var enrichedDto = _resourceService.updateResourceDependencies(taskDTO, projectName);
        CreateTask(enrichedDto);

        var createdTask = _repositoryManager.TaskRepository.Get(t => t.Title == enrichedDto.Title);
        _repositoryManager.ProjectRepository.AddTask(projectName, createdTask);

        RecalculateCriticalPath(projectName);
        var cpmResult = GetCriticalPath(projectName);
        if (cpmResult.CriticalTaskIds.Contains(createdTask.Id))
        {
            var notificationDTO = new NotificationDTO
            {
                Read = false,
                Description = $"The task {createdTask.Title} has been created. The critical path has changed.",
                Project = projectService.GetProjectByName(projectName)
            };
            notificationService.CreateNotification(notificationDTO);
        }
    }

    public void DeleteTask(string projectName, string title)
    {
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        var toDelete = project.Tasks.FirstOrDefault(t => t.Title == title);
        if (toDelete == null) throw new TaskNotFoundException();

        _repositoryManager.ProjectRepository.RemoveTask(projectName, toDelete.Id);
        var entity = _repositoryManager.TaskRepository.Get(t => t.Title == title);
        _repositoryManager.TaskRepository.Delete(entity);

        RecalculateCriticalPath(projectName);
    }

    public void UpdateTask(string projectName, string title, TaskDTO taskDTO, bool solve = false)
    {
        INotificationService notificationService = new NotificationService(_repositoryManager);
        IAdminPService projectService = new AdminPService(_repositoryManager);
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

        var enrichedDto = _resourceService.updateResourceDependencies(taskDTO, projectName);

        List<Task> previousEntities = new List<Task>();
        if (enrichedDto.PreviousTasks != null)
            foreach (var prev in enrichedDto.PreviousTasks)
                if (prev.Id.HasValue)
                {
                    var ent = project.Tasks.FirstOrDefault(t => t.Id == prev.Id.Value);
                    if (ent != null && ent.Title != title) previousEntities.Add(ent);
                }

        List<Task> sameTimeEntities = new List<Task>();
        if (enrichedDto.SameTimeTasks != null)
            foreach (var same in enrichedDto.SameTimeTasks)
                if (same.Id.HasValue)
                {
                    var ent = project.Tasks.FirstOrDefault(t => t.Id == same.Id.Value);
                    if (ent != null && ent.Title != title) sameTimeEntities.Add(ent);
                }

        var updatedTask = new Task(
            enrichedDto.Title,
            enrichedDto.Description,
            enrichedDto.ExpectedStartDate,
            enrichedDto.Duration,
            previousEntities,
            sameTimeEntities,
            _resourceConverter.ToResourceEntityList(enrichedDto.Resources)
        )
        {
            Id = original.Id,
            State = (State)enrichedDto.State
        };

        _repositoryManager.TaskRepository.Update(updatedTask);
        _repositoryManager.ProjectRepository.UpdateTask(projectName, updatedTask.Id, updatedTask);

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
            if (string.IsNullOrEmpty(projectName))
                throw new ArgumentException("Project name cannot be null or empty");

            var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
            if (project == null) throw new ProjectNotFoundException();

            if (project.Tasks == null || !project.Tasks.Any())
                return new List<TaskDTO>();

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
                })
                .ToList();

            if (!taskDTOs.Any())
                return new List<TaskDTO>();

            var taskDict = taskDTOs
                .Where(t => t.Id.HasValue)
                .ToDictionary(t => t.Id.Value, t => t);

            foreach (var t in project.Tasks.Where(t => t.Id.HasValue))
            {
                var dto = taskDict[t.Id.Value];

                if (t.PreviousTasks != null)
                    foreach (var prev in t.PreviousTasks)
                        if (prev.Id.HasValue && taskDict.ContainsKey(prev.Id.Value))
                            dto.PreviousTasks.Add(taskDict[prev.Id.Value]);

                if (t.SameTimeTasks != null)
                    foreach (var same in t.SameTimeTasks)
                        if (same.Id.HasValue && taskDict.ContainsKey(same.Id.Value))
                            dto.SameTimeTasks.Add(taskDict[same.Id.Value]);
            }

            return taskDTOs;
        }
        catch (Exception ex)
        {
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

    private DateTime GetNextDateAvailable(
        bool solve,
        DateTime startDate,
        int duration,
        List<ResourceDTO> resources,
        string taskTitle = ""
    )
    {
        var startDateNext = startDate;
        foreach (var res in resources)
        {
            if (!_resourceService.IsAvailable(res, startDate, duration, taskTitle) && !solve)
                throw new ResourceNotAvailableException();

            var next = _resourceService.NextDateAvailable(res, startDate, duration, taskTitle);
            if (next > startDate) startDateNext = next;
        }

        return startDateNext;
    }

    private void RecalculateCriticalPath(string projectName)
    {
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null || project.Tasks.Count == 0) return;

        try
        {
            List<TaskDTO> updatedTasks = _cpmService
                .CalculateCriticalPath(GetTasks(projectName))
                .AllTasks;

            foreach (var updatedDto in updatedTasks)
            {
                var original = project.Tasks.FirstOrDefault(t => t.Id == updatedDto.Id);
                if (original != null)
                {
                    original.IsCritical = updatedDto.IsCritical;
                    original.StartDate = updatedDto.StartDate;
                    original.EndDate = updatedDto.EndDate;
                    original.LatestStart = updatedDto.LatestStart;
                    original.LatestFinish = updatedDto.LatestFinish;
                    original.Slack = updatedDto.Slack;
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