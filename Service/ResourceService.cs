using DataAccess;
using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;
using Service.Converter;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.ResourceServiceExceptions;
using Service.Interface;
using Service.Models;

namespace Service;

public class ResourceService : IResourceService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ResourceConverter _resourceConverter;
    private readonly RolConverter _rolConverter;
    private readonly TaskConverter _taskConverter;

    public ResourceService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _rolConverter = new RolConverter();
        _resourceConverter = new ResourceConverter(_repositoryManager);
        _taskConverter = new TaskConverter(_repositoryManager);
    }

    public void AddResource(ResourceDTO resourceDTO)
    {
        if (isAdminSystem())
        {
            var resource = _resourceConverter.ToEntity(resourceDTO);
            _repositoryManager.ResourceRepository.Add(resource);
        }
        else
        {
            throw new UnauthorizedAdminAccessException();
        }
    }

    public ResourceDTO Get(int? id)
    {
        var resource = _repositoryManager.ResourceRepository.Get(r => r.Id == id);

        if (resource == null) throw new ResourceNotFoundException();

        return _resourceConverter.FromEntity(resource);
    }

    public List<ResourceDTO> GetResources()
    {
        List<ResourceDTO> resourcesDTO = new List<ResourceDTO>();

        foreach (var resource in _repositoryManager.ResourceRepository.GetAll())
            resourcesDTO.Add(_resourceConverter.FromEntity(resource));

        if (resourcesDTO.Count == 0) throw new NoResourcesFoundException();

        return resourcesDTO;
    }

    public List<ResourceDTO> GetResourcesForProject(string projectName)
    {
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        List<ResourceDTO> resourcesDTO = new List<ResourceDTO>();

        foreach (var resource in _repositoryManager.ResourceRepository.GetAll())
            if (resource.Project == null || resource.Project.Name == projectName)
                resourcesDTO.Add(_resourceConverter.FromEntity(resource));

        if (resourcesDTO.Count == 0) throw new NoResourcesFoundException();

        return resourcesDTO;
    }

    public void UpdateResource(int? id, ResourceDTO updatedResourceDTO)
    {
        var existingResource = GetResourceObject(id);
        isAbleToModifyResource(existingResource);

        var updatedResource = _resourceConverter.ToEntity(updatedResourceDTO);
        updatedResource.Id = id.Value;

        _repositoryManager.ResourceRepository.Update(updatedResource);
    }

    public void DeleteResource(int? id)
    {
        var resource = GetResourceObject(id);
        isAbleToModifyResource(resource);
        try
        {
            _repositoryManager.ResourceRepository.Delete(resource);
        }
        catch (Exception)
        {
            throw new ResourceNotFoundException();
        }
    }

    public TaskDTO updateResourceDependencies(TaskDTO taskDTO, string ProjectName)
    {
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == ProjectName);
        taskDTO.PreviousTasks ??= new List<TaskDTO>();
        taskDTO.Resources ??= new List<ResourceDTO>();
        var resourceIds = taskDTO.Resources
            .Where(r => r.Id.HasValue)
            .Select(r => r.Id.Value)
            .ToHashSet();
        List<Domain.Task> prevTasks = project.Tasks
            .Where(t =>
                t._title != taskDTO.Title &&
                t.ExpectedStartDate < taskDTO.ExpectedStartDate &&
                t.Resources.Any(r => resourceIds.Contains((int)r.Id) && !r.ConcurrentUsage))
            .ToList();
        List<TaskDTO> prevDtos = _taskConverter.ToMinimalTaskDTOList(prevTasks);
        foreach (var prevTaskDTO in prevDtos)
            if (!taskDTO.PreviousTasks.Any(t => t.Id == prevTaskDTO.Id))
                taskDTO.PreviousTasks.Add(prevTaskDTO);

        return taskDTO;
    }

    public bool IsAvailable(ResourceDTO res, DateTime startDate, int duration, string taskTitle = "")
    {
        if (res.ConcurrentUsage)
            return true;
        var endDate = startDate.AddDays(duration);
        IEnumerable<Domain.Task> tasksUsingResource = _repositoryManager.TaskRepository
            .GetAll()
            .Where(t => t.Resources.Any(r => r.Id == res.Id) && t.Title != taskTitle && t.State != State.DONE);
        foreach (var task in tasksUsingResource)
        {
            var taskStart = task.ExpectedStartDate.Date;
            var taskEnd = taskStart.AddDays(task.Duration).Date;
            if (taskStart < endDate && startDate < taskEnd)
                return false;
        }

        return true;
    }

    public DateTime NextDateAvailable(ResourceDTO res, DateTime startDate, int duration, string taskTitle = "")
    {
        if (res.ConcurrentUsage)
            return startDate.Date;
        var candidate = startDate.Date;

        while (!IsAvailable(res, candidate, duration, taskTitle))
            candidate = candidate.AddDays(1);
        return candidate;
    }

    public List<(DateTime, int)> getWhenIsResourceOcupied(ResourceDTO res)
    {
        var whenIsResourceOcupied = new List<(DateTime, int)>();
        IEnumerable<Domain.Task> tasks = _repositoryManager.TaskRepository.GetAll();
        foreach (var task in tasks)
            if (task.Resources.Any(r => r.Id == res.Id))
                whenIsResourceOcupied.Add((task.ExpectedStartDate, task.Duration));

        return whenIsResourceOcupied;
    }

    private Resource GetResourceObject(int? id)
    {
        var resource = _repositoryManager.ResourceRepository.Get(r => r.Id == id);

        if (resource == null) throw new ResourceNotFoundException();

        return resource;
    }

    private void isAbleToModifyResource(Resource resource)
    {
        var currentUser = LoggedUser.Current;

        if (currentUser == null) throw new UnauthorizedAdminAccessException();

        if (currentUser.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.AdminSystem)))
            return;

        if (currentUser.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.AdminProject)) &&
            isExclusive(resource))
            return;

        throw new UnauthorizedAdminAccessException();
    }

    private bool isAdminSystem()
    {
        var currentUser = LoggedUser.Current;
        return currentUser.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.AdminSystem)) ||
               currentUser.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.AdminProject));
    }

    private List<Project> GetProjectsThatAreUsingResource(Resource resource)
    {
        var projects = _repositoryManager.ProjectRepository.GetAll();
        List<Project> projectsThatAreUsingResource = new List<Project>();
        foreach (var project in projects)
        foreach (var task in project.Tasks)
            if (task.Resources.Any(r => r.Id == resource.Id))
            {
                projectsThatAreUsingResource.Add(project);
                break;
            }

        return projectsThatAreUsingResource;
    }

    private bool isExclusive(Resource resource)
    {
        List<Project> projectsUsing = GetProjectsThatAreUsingResource(resource);

        if (projectsUsing.Count != 1)
            return false;

        var project = projectsUsing[0];
        return project.AdminProject != null
               && project.AdminProject.Email.Equals(LoggedUser.Current.Email);
    }

    public List<ResourceDTO> getAllResourcesForAProject(string pName)
    {
        List<ResourceDTO> resources = new List<ResourceDTO>();
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == pName);
        foreach (var task in project.Tasks)
        foreach (var res in task._resources)
            if (!resources.Any(r => r.Id == res.Id))
                resources.Add(_resourceConverter.FromEntity(res));

        return resources;
    }
}