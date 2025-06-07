using DataAccess;
using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;
using Service.Models;
using Task = Domain.Task;

namespace Service.Converter;

public class TaskConverter : IConverter<Task, TaskDTO>
{
    private readonly IRepositoryManager _repositoryManager;

    public TaskConverter(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    public Task ToEntity(TaskDTO taskDTO)
    {
        var taskEntity = new Task(
            taskDTO.Title,
            taskDTO.Description,
            taskDTO.ExpectedStartDate,
            taskDTO.Duration,
            ConvertToEntityList(taskDTO.PreviousTasks),
            ConvertToEntityList(taskDTO.SameTimeTasks),
            ConvertToResourceEntityList(taskDTO.Resources)
        );

        taskEntity.Id = taskDTO.Id;
        taskEntity.State = (State)taskDTO.State;

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
            Resources = ConvertFromResourceEntityList(task.Resources) ?? new List<ResourceDTO>(),
            IsCritical = task.IsCritical,
            StartDate = task.StartDate,
            EndDate = task.EndDate,
            LatestStart = task.LatestStart,
            LatestFinish = task.LatestFinish,
            Slack = task.Slack
        };
    }

    public List<Task> ConvertToEntityList(List<TaskDTO> taskDTOs)
    {
        if (taskDTOs == null) return new List<Task>();
        return taskDTOs.Select(ToEntity).ToList();
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

    private List<Resource> ConvertToResourceEntityList(List<ResourceDTO> resourceDTOs)
    {
        if (resourceDTOs == null) return new List<Resource>();

        List<Resource> resources = new List<Resource>();
        foreach (var resourceDTO in resourceDTOs)
        {
            Resource existing = _repositoryManager.ResourceRepository.Get(r => r.Id == resourceDTO.Id);
            if (existing == null)
                throw new ResourceNotFoundException();
            resources.Add(existing);
        }

        return resources;
    }

    private List<ResourceDTO> ConvertFromResourceEntityList(List<Resource> resources)
    {
        if (resources == null) return new List<ResourceDTO>();

        return resources.Select(resource => new ResourceDTO
        {
            Id = resource.Id,
            Name = resource.Name,
            Type = resource.Type,
            Description = resource.Description
        }).ToList();
    }
}
