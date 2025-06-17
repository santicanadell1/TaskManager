using DataAccess;
using Service;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class ResourceController
{
    private readonly IResourceService _resourceService;

    public ResourceController(IRepositoryManager repositoryManager)
    {
        _resourceService = new ResourceService(repositoryManager);
    }

    public void CreateResource(ResourceDTO resource)
    {
        _resourceService.AddResource(resource);
    }

    public void DeleteResource(int? idResource)
    {
        _resourceService.DeleteResource(idResource);
    }

    public void UpdateResource(int? idResourceToUpdate, ResourceDTO updatedResource)
    {
        _resourceService.UpdateResource(idResourceToUpdate, updatedResource);
    }

    public List<ResourceDTO> GetAllResources()
    {
        return _resourceService.GetResources();
    }

    public List<ResourceDTO> GetAllResourcesForAProject(string projectName)
    {
        return _resourceService.GetResourcesForProject(projectName);
    }
}