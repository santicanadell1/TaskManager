using DataAccess;
using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.ResourceServiceExceptions;
using Service.Interface;
using Service.Models;
using Service.Models.Exceptions;

namespace Service;

public class ResourceService : IResourceService
{
    private readonly ResourceRepository _resourceRepository;
    private readonly ProjectRepository _projectRepository;

    public ResourceService(ResourceRepository resourceRepository, ProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
        _resourceRepository = resourceRepository;
    }

    public void AddResource(ResourceDTO resourceDTO)
    {
        if (isAdminSystem())
        {
            var resource = ToEntity(resourceDTO);
            _resourceRepository.Add(resource);
        }
        else
        {
            throw new UnauthorizedAdminAccessException();
        }
    }

    public ResourceDTO Get(int? id)
    {
        var resource = _resourceRepository.Get(r => r.Id == id);

        if (resource == null) throw new ResourceNotFoundException();

        return FromEntity(resource);
    }

    public List<ResourceDTO> GetResources()
    {
        List<ResourceDTO> resourcesDTO = new List<ResourceDTO>();

        foreach (var resource in _resourceRepository.GetAll()) resourcesDTO.Add(FromEntity(resource));

        if (resourcesDTO.Count == 0) throw new NoResourcesFoundException();

        return resourcesDTO;
    }

    public void UpdateResource(int? id, ResourceDTO updatedResourceDTO)
    {
        isAbleToModifyResource(GetResourceObject(id));

        var updatedResource = ToEntity(updatedResourceDTO);
        updatedResource.Id = id.Value;
        
        _resourceRepository.Update(updatedResource);
    }

    public void DeleteResource(int? id)
    {
        isAbleToModifyResource(GetResourceObject(id));
        try
        {
            var res = _resourceRepository.Get(r => r.Id == id);
            _resourceRepository.Delete(res);
        }
        catch (Exception ex)
        {
            throw new ResourceNotFoundException();
        }
    }

    private Resource GetResourceObject(int? id)
    {
        var resource = _resourceRepository.Get(r => r.Id == id);

        if (resource == null) throw new ResourceNotFoundException();

        return resource;
    }


    private ResourceDTO FromEntity(Resource resource)
    {
        return new ResourceDTO
        {
            Name = resource.Name,
            Type = resource.Type,
            Description = resource.Description,
            Id = resource.Id
        };
    }

    private Resource ToEntity(ResourceDTO resourceDTO)
    {
        var resource = new Resource(resourceDTO.Name, resourceDTO.Type, resourceDTO.Description)
        {
            Id = resourceDTO.Id
        };
        return resource;
    }

    private void isAbleToModifyResource(Resource resource)
    {
        var currentUser = LoggedUser.Current;

        if (currentUser == null) throw new UnauthorizedAdminAccessException();

        if (currentUser.Roles.Contains(ConvertToDTORole(Rol.AdminSystem))) return;

        if (currentUser.Roles.Contains(ConvertToDTORole(Rol.AdminProject)) && isExclusive(resource)) return;

        throw new UnauthorizedAdminAccessException();
    }

    private bool isAdminSystem()
    {
        var currentUser = LoggedUser.Current;
        return currentUser.Roles.Contains(ConvertToDTORole(Rol.AdminSystem));
    }

    private List<Project> GetProjectsThatAreUsingResource(Resource resource)
    {
        var projects = _projectRepository.GetAll();
        List<Project> projectsThatAreUsingResource = new List<Project>();
        foreach (var project in projects)
        {
            foreach (var task in project.Tasks)
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
        var currentUser = LoggedUser.Current;
        List<Project> projects = GetProjectsThatAreUsingResource(resource);
        if (projects.Count == 0) return false;
        var currentUserIsAdmin = currentUser.Roles.Contains(ConvertToDTORole(Rol.AdminProject));
        var isUsedByOneProject = projects.Count == 1;
        var projectAdminIsCurrentUser = projects[0].AdminProject.Email.Equals(currentUser.Email);
        return currentUserIsAdmin && isUsedByOneProject && projectAdminIsCurrentUser;
    }

    private RolDTO ConvertToDTORole(Rol role)
    {
        switch (role)
        {
            case Rol.AdminSystem:
                return RolDTO.AdminSystem;
            case Rol.ProjectMember:
                return RolDTO.ProjectMember;
            case Rol.AdminProject:
                return RolDTO.AdminProject;
            default:
                throw new InvalidRolException();
        }
    }
}