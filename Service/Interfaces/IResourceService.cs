using Service.Models;

namespace Service.Interface;

public interface IResourceService
{
    void AddResource(ResourceDTO resourceDTO);
    ResourceDTO Get(int? id);
    List<ResourceDTO> GetResources();
    void UpdateResource(int? id, ResourceDTO updatedResourceDTO);
    void DeleteResource(int? id);
    bool IsAvailable(ResourceDTO resourceDTO, DateTime startDate, int duration, string taskTitle = "");
    DateTime NextDateAvailable(ResourceDTO resourceDTO, DateTime startDate, int duration, string taskTitle = "");
    public List<ResourceDTO> GetResourcesForProject(string pName);
    public List<(DateTime, int)> getWhenIsResourceOcupied(ResourceDTO res);

    public TaskDTO updateResourceDependencies(TaskDTO taskDTO, string ProjectName);
}