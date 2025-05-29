using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Domain.Exceptions.TaskExceptions;
using Service.Models;
using Task = Domain.Task;

namespace Service;

public class TaskService
{
    private readonly CpmService _cpmService;
    private readonly ProjectRepository _projectRepository;
    private readonly UserRepository _userRepository;
    private readonly NotificationRepository _notificationRepository;

    public TaskService(ProjectRepository projectRepository, NotificationRepository notificationRepository, UserRepository userRepository,CpmService cpmService)
    {
        _projectRepository = projectRepository;
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _cpmService = cpmService;
    }

    public void AddTask(string projectName, TaskDTO taskDTO)
    {
        var project = _projectRepository.GetProject(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        if (taskDTO.ExpectedStartDate.AddDays(1) <= project.StartDate)
            throw new TaskException("The task's start date is before the project's start date.");
        var previousTasks = new List<Task>();
        var sameTimeTasks = new List<Task>();

        if (taskDTO.PreviousTasks != null)
            foreach (var prevTaskDTO in taskDTO.PreviousTasks)
                if (prevTaskDTO.Id.HasValue)
                {
                    var existingTask = project.Tasks.FirstOrDefault(t => t.Id == prevTaskDTO.Id);
                    if (existingTask != null) previousTasks.Add(existingTask);
                }

        if (taskDTO.SameTimeTasks != null)
            foreach (var sameTaskDTO in taskDTO.SameTimeTasks)
                if (sameTaskDTO.Id.HasValue)
                {
                    var existingTask = project.Tasks.FirstOrDefault(t => t.Id == sameTaskDTO.Id);
                    if (existingTask != null) sameTimeTasks.Add(existingTask);
                }

        var task = new Task(
            taskDTO.Title,
            taskDTO.Description,
            taskDTO.ExpectedStartDate,
            taskDTO.Duration,
            previousTasks,
            sameTimeTasks,
            ToResourceEntityList(taskDTO.Resources)
        );

        _projectRepository.AddTask(projectName, task);

        RecalculateCriticalPath(projectName);
    }

    public void DeleteTask(string projectName, int? taskId)
    {
        var project = _projectRepository.GetProject(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null) throw new TaskNotFoundException();

        _projectRepository.RemoveTask(projectName, task.Id);

        RecalculateCriticalPath(projectName);
    }

    public void UpdateTask(string projectName, int? taskId, TaskDTO taskDTO)
    {
        var _notificationService = new NotificationService(_userRepository, _projectRepository, _notificationRepository);;
        var projectService = new AdminPService(_userRepository,_projectRepository,_notificationRepository);
        var project = _projectRepository.GetProject(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null) throw new TaskNotFoundException();

        var previousTasks = new List<Task>();
        if (taskDTO.PreviousTasks != null)
            foreach (var prevTaskDTO in taskDTO.PreviousTasks)
                if (prevTaskDTO.Id.HasValue)
                {
                    var existingTask = project.Tasks.FirstOrDefault(t => t.Id == prevTaskDTO.Id);
                    if (existingTask != null && existingTask.Id != taskId) previousTasks.Add(existingTask);
                }

        var sameTimeTasks = new List<Task>();
        if (taskDTO.SameTimeTasks != null)
            foreach (var sameTaskDTO in taskDTO.SameTimeTasks)
                if (sameTaskDTO.Id.HasValue)
                {
                    var existingTask = project.Tasks.FirstOrDefault(t => t.Id == sameTaskDTO.Id);
                    if (existingTask != null && existingTask.Id != taskId) sameTimeTasks.Add(existingTask);
                }

        var updatedTask = new Task(
            taskDTO.Title,
            taskDTO.Description,
            taskDTO.ExpectedStartDate,
            taskDTO.Duration,
            previousTasks,
            sameTimeTasks,
            ToResourceEntityList(taskDTO.Resources)
        );
        updatedTask.Id = task.Id;

        updatedTask.State = (State)taskDTO.State;

        _projectRepository.UpdateTask(projectName, taskId, updatedTask);

        RecalculateCriticalPath(projectName);
        var cpmResult = GetCriticalPath(projectName);
        if (cpmResult.CriticalTaskIds.Any(t => t == taskId))
        {
            var notificationDTO = new NotificationDTO
            {
                Read = false,
                Description = $"The task {updatedTask.Title} has been updated. The critical path has changed.",
                Project = projectService.GetProjectByName(projectName)
            };
            _notificationService.CreateNotification(notificationDTO);
        }
    }

    public List<TaskDTO> GetTasks(string projectName)
    {
        var project = _projectRepository.GetProject(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        var taskDTOs = project.Tasks.Select(t => new TaskDTO
        {
            Title = t.Title,
            Description = t.Description,
            ExpectedStartDate = t.ExpectedStartDate,
            Duration = t.Duration,
            State = (StateDTO)t.State,
            Resources = FromResourceEntityList(t.Resources),
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

        var taskDict = taskDTOs.ToDictionary(t => t.Id);

        foreach (var task in project.Tasks)
        {
            var taskDto = taskDict[task.Id];

            foreach (var prevTask in task.PreviousTasks)
                if (taskDict.ContainsKey(prevTask.Id))
                    taskDto.PreviousTasks.Add(taskDict[prevTask.Id]);

            foreach (var sameTask in task.SameTimeTasks)
                if (taskDict.ContainsKey(sameTask.Id))
                    taskDto.SameTimeTasks.Add(taskDict[sameTask.Id]);
        }

        return taskDTOs;
    }

    public TaskDTO GetTask(string projectName, int? taskId)
    {
        var project = _projectRepository.GetProject(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null) throw new TaskNotFoundException();

        return FromEntity(task);
    }

    public CpmResultDTO GetCriticalPath(string projectName)
    {
        var project = _projectRepository.GetProject(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        var cpmResult = _cpmService.CalculateCriticalPath(GetTasks(projectName));

        return new CpmResultDTO
        {
            ProjectDuration = cpmResult.ProjectDuration,
            CriticalTaskIds = cpmResult.CriticalTasks.Select(t => t.Id).ToList(),
            CriticalPathIds = cpmResult.CriticalPath.Select(t => t.Id).ToList(),
            EarliestStartDate = project.Tasks.Min(t => t.StartDate),
            LatestFinishDate = project.Tasks.Max(t => t.EndDate)
        };
    }

    private void RecalculateCriticalPath(string projectName)
    {
        var project = _projectRepository.GetProject(p => p.Name == projectName);
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


    private TaskDTO FromEntity(Task task)
    {
        return new TaskDTO
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            ExpectedStartDate = task.ExpectedStartDate,
            Duration = task.Duration,
            PreviousTasks = ToMinimalTaskDTOList(task.PreviousTasks),
            SameTimeTasks = ToMinimalTaskDTOList(task.SameTimeTasks),
            State = (StateDTO)task.State,
            Resources = FromResourceEntityList(task.Resources) ?? new List<ResourceDTO>(),
            IsCritical = task.IsCritical,
            StartDate = task.StartDate,
            EndDate = task.EndDate,
            LatestStart = task.LatestStart,
            LatestFinish = task.LatestFinish,
            Slack = task.Slack
        };
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

    private List<TaskDTO> FromEntityList(List<Task> tasks)
    {
        if (tasks == null) return new List<TaskDTO>();

        var taskDTOs = new List<TaskDTO>();
        foreach (var task in tasks)
            taskDTOs.Add(new TaskDTO
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                ExpectedStartDate = task.ExpectedStartDate,
                Duration = task.Duration,
                State = (StateDTO)task.State,
                PreviousTasks = new List<TaskDTO>(),
                SameTimeTasks = new List<TaskDTO>(),
                Resources = new List<ResourceDTO>()
            });

        return taskDTOs;
    }

    private List<ResourceDTO> FromResourceEntityList(List<Resource> resources)
    {
        var resourceDTOs = new List<ResourceDTO>();
        foreach (var resource in resources)
            resourceDTOs.Add(new ResourceDTO
            {
                Name = resource.Name,
                Type = resource.Type,
                Description = resource.Description,
                Id = resource.Id
            });

        return resourceDTOs;
    }

    private Task ToEntity(TaskDTO taskDTO)
    {
        return new Task(
            taskDTO.Title,
            taskDTO.Description,
            taskDTO.ExpectedStartDate,
            taskDTO.Duration,
            ToEntityList(taskDTO.PreviousTasks),
            ToEntityList(taskDTO.SameTimeTasks),
            ToResourceEntityList(taskDTO.Resources)
        );
    }

    private List<Task> ToEntityList(List<TaskDTO> taskDTOs)
    {
        if (taskDTOs == null) return new List<Task>();

        var tasks = new List<Task>();
        foreach (var taskDTO in taskDTOs) tasks.Add(ToEntity(taskDTO));

        return tasks;
    }

    private List<Resource> ToResourceEntityList(List<ResourceDTO> resourceDTOs)
    {
        if (resourceDTOs == null) return new List<Resource>();

        var resources = new List<Resource>();
        foreach (var resourceDTO in resourceDTOs)
            resources.Add(new Resource(resourceDTO.Name, resourceDTO.Type, resourceDTO.Description)
                { Id = resourceDTO.Id });

        return resources;
    }
}