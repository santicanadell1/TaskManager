using Service.Models;
using System.Collections.Generic;

namespace Service.Interface;

public interface IResourceService
{
    void AddResource(ResourceDTO resourceDTO);
    ResourceDTO Get(int? id);
    List<ResourceDTO> GetResources();
    void UpdateResource(int? id, ResourceDTO updatedResourceDTO);
    void DeleteResource(int? id);
}