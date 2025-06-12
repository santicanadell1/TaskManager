using Service.Interface;

namespace Controllers;

public class ResourceAdminController
{
    private readonly IResourceService _resourceService;

    public ResourceAdminController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }
}