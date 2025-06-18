using DataAccess;
using Domain;
using Service.Models;
using Task = Domain.Task;

namespace Service.Converter;

public class TaskConverter : IConverter<Task, TaskDTO>
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ResourceConverter _resourceConverter;

    public TaskConverter(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _resourceConverter = new ResourceConverter(repositoryManager);
    }

    public Task ToEntity(TaskDTO taskDTO)
    {
        var taskEntity = new Task(
            taskDTO.Title,
            taskDTO.Description,
            taskDTO.ExpectedStartDate,
            taskDTO.Duration,
            GetExistingTasksFromIds(taskDTO.PreviousTasks),
            GetExistingTasksFromIds(taskDTO.SameTimeTasks),
            _resourceConverter.ConvertToResourceEntityList(taskDTO.Resources)
        );

        taskEntity.Id = taskDTO.Id;
        taskEntity.State = (State)taskDTO.State;
        taskEntity.IsCritical = taskDTO.IsCritical;
        taskEntity.StartDate = taskDTO.StartDate;
        taskEntity.EndDate = taskDTO.EndDate;
        taskEntity.LatestStart = taskDTO.LatestStart;
        taskEntity.LatestFinish = taskDTO.LatestFinish;
        taskEntity.Slack = taskDTO.Slack;

        return taskEntity;
    }

    public TaskDTO FromEntity(Task task)
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
            Resources = _resourceConverter.ConvertFromResourceEntityList(task.Resources) ?? new List<ResourceDTO>(),
            IsCritical = task.IsCritical,
            StartDate = task.StartDate,
            EndDate = task.EndDate,
            LatestStart = task.LatestStart,
            LatestFinish = task.LatestFinish,
            Slack = task.Slack
        };
    }

    public List<Task> GetExistingTasksFromIds(List<TaskDTO> taskDTOs)
    {
        if (taskDTOs == null) return new List<Task>();

        var tasks = new List<Task>();
        foreach (var taskDTO in taskDTOs)
            if (taskDTO.Id.HasValue)
            {
                var existingTask = _repositoryManager.TaskRepository.Get(t => t.Id == taskDTO.Id);
                if (existingTask != null) tasks.Add(existingTask);
            }

        return tasks;
    }

    public List<Task> ConvertToEntityList(List<TaskDTO> taskDTOs)
    {
        if (taskDTOs == null) return new List<Task>();

        var tasks = new List<Task>();
        foreach (var taskDTO in taskDTOs) tasks.Add(ToEntity(taskDTO));

        return tasks;
    }

    public List<TaskDTO> ConvertFromEntityList(List<Task> tasks)
    {
        if (tasks == null) return new List<TaskDTO>();
        return tasks.Select(FromEntity).ToList();
    }

    public List<TaskDTO> ToMinimalTaskDTOList(List<Task> tasks)
    {
        if (tasks == null) return new List<TaskDTO>();
        return tasks.Select(t => new TaskDTO
        {
            Id = t.Id,
            Title = t.Title
        }).ToList();
    }
}