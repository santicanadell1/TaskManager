using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.ResourceRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Domain.Exceptions.TaskExceptions;
using Service.Models;
using Task = Domain.Task;

namespace Service;

public class TaskService
{
    private readonly CpmService _cpmService;
    private readonly IRepositoryManager _repositoryManager;

    public TaskService(IRepositoryManager repositoryManager, CpmService cpmService)
    {
        _repositoryManager = repositoryManager;
        _cpmService = cpmService;
    }

    private void CreateTask(TaskDTO taskDTO)
    {
        Task task = ToEntity(taskDTO);
        _repositoryManager.TaskRepository.Add(task);
    }

    public void UpdateTask(TaskDTO taskDTO)
    {
        Task task = ToEntity(taskDTO);
        _repositoryManager.TaskRepository.Update(task);
    }

    public void DeleteTask(TaskDTO taskDTO)
    {
        Task task = ToEntity(taskDTO);
        _repositoryManager.TaskRepository.Delete(task);
    }

    public TaskDTO GetTask(string title)
    {
        return FromEntity(_repositoryManager.TaskRepository.Get(t => t.Title == title));
    }

    public List<TaskDTO> GetTasks()
    {
        List<TaskDTO> tasks = new List<TaskDTO>();
        foreach (Task task in _repositoryManager.TaskRepository.GetAll())
        {
            tasks.Add(FromEntity(task));
        }

        return tasks;
    }

    public void AddTask(string projectName, TaskDTO taskDTO)
    {
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();
        if (taskDTO.ExpectedStartDate.AddDays(1) <= project.StartDate)
        {
            throw new TaskException("The task's start date is before the project's start date.");
        }

        CreateTask(taskDTO);
        Task task = _repositoryManager.TaskRepository.Get(t => t.Title == taskDTO.Title);

        project.Tasks.Add(task);

        _repositoryManager.ProjectRepository.Update(project);

        RecalculateCriticalPath(projectName);
    }

    public void DeleteTask(string projectName, string title)
    {
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        Task task = project.Tasks.FirstOrDefault(t => t.Title == title);
        if (task == null) throw new TaskNotFoundException();

        _repositoryManager.ProjectRepository.RemoveTask(projectName, task.Id);
        Task taskEntity = _repositoryManager.TaskRepository.Get(t => t.Title == title);
        _repositoryManager.TaskRepository.Delete(taskEntity);

        RecalculateCriticalPath(projectName);
    }

    public void UpdateTask(string projectName, string title, TaskDTO taskDTO)
    {
        NotificationService _notificationService = new NotificationService(_repositoryManager);
        AdminPService projectService = new AdminPService(_repositoryManager);
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        Task task = project.Tasks.FirstOrDefault(t => t.Title == title);
        if (task == null) throw new TaskNotFoundException();

        List<Task> previousTasks = new List<Task>();
        if (taskDTO.PreviousTasks != null)
            foreach (TaskDTO prevTaskDTO in taskDTO.PreviousTasks)
                if (prevTaskDTO.Id.HasValue)
                {
                    Task existingTask = project.Tasks.FirstOrDefault(t => t.Id == prevTaskDTO.Id);
                    if (existingTask != null && existingTask.Title != title) previousTasks.Add(existingTask);
                }

        List<Task> sameTimeTasks = new List<Task>();
        if (taskDTO.SameTimeTasks != null)
            foreach (TaskDTO sameTaskDTO in taskDTO.SameTimeTasks)
                if (sameTaskDTO.Id.HasValue)
                {
                    Task existingTask = project.Tasks.FirstOrDefault(t => t.Id == sameTaskDTO.Id);
                    if (existingTask != null && existingTask.Title != title) sameTimeTasks.Add(existingTask);
                }

        Task updatedTask = new Task(
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
        _repositoryManager.TaskRepository.Update(updatedTask);
        updatedTask = _repositoryManager.TaskRepository.Get(t => t.Title == updatedTask.Title);
        _repositoryManager.ProjectRepository.UpdateTask(projectName, updatedTask.Id, updatedTask);

        RecalculateCriticalPath(projectName);
        CpmResultDTO cpmResult = GetCriticalPath(projectName);
        if (cpmResult.CriticalTaskIds.Any(t => t == updatedTask.Id))
        {
            NotificationDTO notificationDTO = new NotificationDTO
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
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        List<TaskDTO> taskDTOs = project.Tasks.Select(t => new TaskDTO
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

        Dictionary<int?, TaskDTO> taskDict = taskDTOs.ToDictionary(t => t.Id);

        foreach (Task task in project.Tasks)
        {
            TaskDTO taskDto = taskDict[task.Id];

            foreach (Task prevTask in task.PreviousTasks)
                if (taskDict.ContainsKey(prevTask.Id))
                    taskDto.PreviousTasks.Add(taskDict[prevTask.Id]);

            foreach (Task sameTask in task.SameTimeTasks)
                if (taskDict.ContainsKey(sameTask.Id))
                    taskDto.SameTimeTasks.Add(taskDict[sameTask.Id]);
        }

        return taskDTOs;
    }

    public TaskDTO GetTask(string projectName, string title)
    {
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        Task task = project.Tasks.FirstOrDefault(t => t.Title == title);
        if (task == null) throw new TaskNotFoundException();

        return FromEntity(task);
    }

    public CpmResultDTO GetCriticalPath(string projectName)
    {
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        CpmResult cpmResult = _cpmService.CalculateCriticalPath(GetTasks(projectName));

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
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null || project.Tasks.Count == 0) return;

        try
        {
            List<TaskDTO> updatedTasks = _cpmService.CalculateCriticalPath(GetTasks(projectName)).AllTasks;

            foreach (TaskDTO updatedDTO in updatedTasks)
            {
                Task originalTask = project.Tasks.FirstOrDefault(t => t.Id == updatedDTO.Id);
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

    private List<ResourceDTO> FromResourceEntityList(List<Resource> resources)
    {
        List<ResourceDTO> resourceDTOs = new List<ResourceDTO>();
        foreach (Resource resource in resources)
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
        Task TaskEntity = new Task(
            taskDTO.Title,
            taskDTO.Description,
            taskDTO.ExpectedStartDate,
            taskDTO.Duration,
            ToEntityList(taskDTO.PreviousTasks),
            ToEntityList(taskDTO.SameTimeTasks),
            ToResourceEntityList(taskDTO.Resources)
        );
        if (TaskEntity.Id.HasValue)
        {
            TaskEntity.Id = taskDTO.Id;
        }

        TaskEntity.State = (State)taskDTO.State;
        return TaskEntity;
    }

    private List<Task> ToEntityList(List<TaskDTO> taskDTOs)
    {
        if (taskDTOs == null) return new List<Task>();

        List<Task> tasks = new List<Task>();
        foreach (TaskDTO taskDTO in taskDTOs) tasks.Add(ToEntity(taskDTO));

        return tasks;
    }

    private List<Resource> ToResourceEntityList(List<ResourceDTO> resourceDTOs)
    {
        if (resourceDTOs == null) return new List<Resource>();

        List<Resource> resources = new List<Resource>();
        foreach (ResourceDTO resourceDTO in resourceDTOs)
        {
            Resource existing = _repositoryManager.ResourceRepository.Get(r => r.Id == resourceDTO.Id);
            if (existing == null)
                throw new ResourceNotFoundException();
            resources.Add(existing);
        }

        return resources;
    }
}