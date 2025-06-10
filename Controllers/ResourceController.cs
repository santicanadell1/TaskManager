using DataAccess;
using Domain;
using Service;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class ResourceController
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IResourceService _resourceService;

    public ResourceController(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
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

    public void UpdateResource(int? idResourceToUpdate,ResourceDTO updatedResource)
    {
        _resourceService.UpdateResource(idResourceToUpdate, updatedResource);
    }

    public List<ResourceDTO> GetAllResources()
    {
        return _resourceService.GetResources();
    }
}