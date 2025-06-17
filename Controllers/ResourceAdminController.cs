using DataAccess;
using Service;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class ResourceAdminController
{
    private readonly IResourceService _resourceService;

    public ResourceAdminController(IRepositoryManager repositoryManager)
    {
        _resourceService = new ResourceService(repositoryManager);
    }

    public List<ResourceDTO> getAllResourcesForAProject(string pName)
    {
        return _resourceService.GetResourcesForProject(pName);
    }

    public List<(DateTime, int)> getWhenIsResourceOcupied(ResourceDTO res)
    {
        return _resourceService.getWhenIsResourceOcupied(res);
    }

    public DateTime NextDateAvailable(ResourceDTO res, DateTime startDate, int duration)
    {
        return _resourceService.NextDateAvailable(res, startDate, duration);
    }
}