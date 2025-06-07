using Domain;
using Service.Models;

namespace Service.Converter;

public class ResourceConverter : IConverter<Resource, ResourceDTO>
{
    public ResourceDTO FromEntity(Resource resource)
    {
        return new ResourceDTO
        {
            Id = resource.Id,
            Name = resource.Name,
            Type = resource.Type,
            Description = resource.Description
        };
    }

    public Resource ToEntity(ResourceDTO resourceDTO)
    {
        return new Resource(resourceDTO.Name, resourceDTO.Type, resourceDTO.Description)
        {
            Id = resourceDTO.Id
        };
    }
}