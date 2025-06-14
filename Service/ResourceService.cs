using DataAccess;
using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;
using Service.Converter;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.ResourceServiceExceptions;
using Service.Interface;
using Service.Models;
using Service.Models.Exceptions;
using Task = Domain.Task;

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
            Resource resource = _resourceConverter.ToEntity(resourceDTO);
            _repositoryManager.ResourceRepository.Add(resource);
        }
        else
        {
            throw new UnauthorizedAdminAccessException();
        }
    }

    public ResourceDTO Get(int? id)
    {
        Resource resource = _repositoryManager.ResourceRepository.Get(r => r.Id == id);

        if (resource == null) throw new ResourceNotFoundException();

        return _resourceConverter.FromEntity(resource);
    }

    public List<ResourceDTO> GetResources()
    {
        List<ResourceDTO> resourcesDTO = new List<ResourceDTO>();

        foreach (Resource resource in _repositoryManager.ResourceRepository.GetAll())
            resourcesDTO.Add(_resourceConverter.FromEntity(resource));

        if (resourcesDTO.Count == 0) throw new NoResourcesFoundException();

        return resourcesDTO;
    }

    public void UpdateResource(int? id, ResourceDTO updatedResourceDTO)
    {
        isAbleToModifyResource(GetResourceObject(id));

        Resource updatedResource = _resourceConverter.ToEntity(updatedResourceDTO);
        updatedResource.Id = id.Value;

        _repositoryManager.ResourceRepository.Update(updatedResource);
    }

    public void DeleteResource(int? id)
    {
        isAbleToModifyResource(GetResourceObject(id));
        try
        {
            Resource? res = _repositoryManager.ResourceRepository.Get(r => r.Id == id);
            _repositoryManager.ResourceRepository.Delete(res);
        }
        catch (Exception ex)
        {
            throw new ResourceNotFoundException();
        }
    }

    private Resource GetResourceObject(int? id)
    {
        Resource resource = _repositoryManager.ResourceRepository.Get(r => r.Id == id);

        if (resource == null) throw new ResourceNotFoundException();

        return resource;
    }

    private void isAbleToModifyResource(Resource resource)
    {
        UserDTO currentUser = LoggedUser.Current;

        if (currentUser == null) throw new UnauthorizedAdminAccessException();

        if (currentUser.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.AdminSystem))) return;

        if (currentUser.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.AdminProject)) &&
            isExclusive(resource)) return;

        throw new UnauthorizedAdminAccessException();
    }

    private bool isAdminSystem()
    {
        UserDTO currentUser = LoggedUser.Current;
        return currentUser.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.AdminSystem));
    }

    private List<Project> GetProjectsThatAreUsingResource(Resource resource)
    {
        List<Project> projects = _repositoryManager.ProjectRepository.GetAll();
        List<Project> projectsThatAreUsingResource = new List<Project>();
        foreach (Project project in projects)
        {
            foreach (Task task in project.Tasks)
            {
                if (task.Resources.Any(r => r.Id == resource.Id))
                {
                    projectsThatAreUsingResource.Add(project);
                    break;
                }
            }
        }

        return projectsThatAreUsingResource;
    }

    private bool isExclusive(Resource resource)
    {
        UserDTO currentUser = LoggedUser.Current;
        List<Project> projects = GetProjectsThatAreUsingResource(resource);
        if (projects.Count == 0) return false;
        bool currentUserIsAdmin = currentUser.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.AdminProject));
        bool isUsedByOneProject = projects.Count == 1;
        bool projectAdminIsCurrentUser = projects[0].AdminProject.Email.Equals(currentUser.Email);
        return currentUserIsAdmin && isUsedByOneProject && projectAdminIsCurrentUser;
    }

    public TaskDTO updateResourceDependencies(TaskDTO taskDTO, string ProjectName)
    {
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == ProjectName);
        taskDTO.PreviousTasks ??= new List<TaskDTO>();
        taskDTO.Resources     ??= new List<ResourceDTO>();
        HashSet<int> resourceIds = taskDTO.Resources
            .Where(r => r.Id.HasValue)
            .Select(r => r.Id.Value)
            .ToHashSet();
        List<Task> prevTasks = project.Tasks
            .Where(t =>
                t._title != taskDTO.Title &&
                t.ExpectedStartDate < taskDTO.ExpectedStartDate &&
                t.Resources.Any(r => resourceIds.Contains((int)r.Id) && !r.ConcurrentUsage))
            .ToList();
        List<TaskDTO> prevDtos = _taskConverter.ToMinimalTaskDTOList(prevTasks);
        foreach (TaskDTO prevTaskDTO in prevDtos)
        {
            if(taskDTO.PreviousTasks.Any(t => t.Id == prevTaskDTO.Id) == false)
                taskDTO.PreviousTasks.Add(prevTaskDTO);
        }

        return taskDTO;
    }

    public bool IsAvailable(ResourceDTO res, DateTime startDate, int duration, string taskTitle = "")
    {
        if (res.ConcurrentUsage)
            return true;
        DateTime endDate = startDate.AddDays(duration);
        var tasksUsingResource = _repositoryManager.TaskRepository
            .GetAll()
            .Where(t => t.Resources.Any(r => r.Id == res.Id) && t.Title != taskTitle && t.State != State.DONE);
        foreach (var task in tasksUsingResource)
        {
            DateTime taskStart = task.ExpectedStartDate.Date;
            DateTime taskEnd   = taskStart.AddDays(task.Duration).Date;
            if (taskStart < endDate && startDate < taskEnd)
                return false;
        }
        return true;
    }

    public DateTime NextDateAvailable(ResourceDTO res, DateTime startDate, int duration, string taskTitle = "")
    {
        if (res.ConcurrentUsage)
            return startDate.Date;
        DateTime candidate = startDate.Date;

        while (!IsAvailable(res, candidate, duration, taskTitle))
        {
            candidate = candidate.AddDays(1);
        }
        return candidate;
    }

    public List<ResourceDTO> getAllResourcesForAProject(string pName)
    {
        List<ResourceDTO> resources = new List<ResourceDTO>();
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == pName);
        foreach (Task task in project.Tasks)
        {
            foreach (Resource res in task._resources)
            {
                if (!resources.Any(r => r.Id == res.Id))
                {
                    resources.Add(_resourceConverter.FromEntity(res));
                }
            }
        }
        return resources;
    }
    
    public List<(DateTime, int)> getWhenIsResourceOcupied(ResourceDTO res)
    {
        List<(DateTime, int)> whenIsResourceOcupied = new List<(DateTime, int)>();
        List<Task> tasks = _repositoryManager.TaskRepository.GetAll();
        foreach (Task task in tasks)
        {
            if (task.Resources.Any(r => r.Id == res.Id))
            {
                whenIsResourceOcupied.Add((task.ExpectedStartDate, task.Duration));
            }
        }
        return whenIsResourceOcupied;
    }
}