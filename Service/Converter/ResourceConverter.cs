using DataAccess;
using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;
using Service.Models;

namespace Service.Converter;

public class ResourceConverter : IConverter<Resource, ResourceDTO>
{
    private IRepositoryManager _repositoryManager;

    public ResourceConverter(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    public ResourceDTO FromEntity(Resource resource)
    {
        return new ResourceDTO
        {
            Id = resource.Id,
            Name = resource.Name,
            Type = resource.Type,
            Description = resource.Description,
            ConcurrentUsage = resource.ConcurrentUsage,
        };
    }

    public Resource ToEntity(ResourceDTO resourceDTO)
    {
        return new Resource(resourceDTO.Name, resourceDTO.Type, resourceDTO.Description)
        {
            Id = resourceDTO.Id,
            ConcurrentUsage = resourceDTO.ConcurrentUsage,
        };
    }

    public List<Resource> ConvertToResourceEntityList(List<ResourceDTO> resourceDTOs)
    {
        if (resourceDTOs == null) return new List<Resource>();

        List<Resource> resources = new List<Resource>();
        foreach (var resourceDTO in resourceDTOs)
        {
            Resource existing = _repositoryManager.ResourceRepository.Get(r => r.Id == resourceDTO.Id);
            if (existing == null)
                throw new ResourceNotFoundException();
            resources.Add(existing);
        }

        return resources;
    }

    public List<ResourceDTO> ConvertFromResourceEntityList(List<Resource> resources)
    {
        if (resources == null)
            return new List<ResourceDTO>();

        List<ResourceDTO> resourceDTOs = new List<ResourceDTO>();
        foreach (Resource resource in resources)
            resourceDTOs.Add(new ResourceDTO
            {
                Name = resource.Name,
                Type = resource.Type,
                Description = resource.Description,
                ConcurrentUsage = resource.ConcurrentUsage,
                Id = resource.Id
            });

        return resourceDTOs;
    }

    public List<Resource> ToResourceEntityList(List<ResourceDTO> resourceDTOs)
    {
        if (resourceDTOs == null) return new List<Resource>();

        List<Resource> resources = new List<Resource>();
        foreach (ResourceDTO resourceDTO in resourceDTOs)
        {
            Resource existing = _repositoryManager.ResourceRepository.Get(r => r.Id == resourceDTO.Id);
            if (existing == null)
                throw new ResourceNotFoundException();
            resources.Add(existing);
        }

        return resources;
    }

    public List<ResourceDTO> FromResourceEntityList(List<Resource> resources)
    {
        if (resources == null)
            return new List<ResourceDTO>();

        List<ResourceDTO> resourceDTOs = new List<ResourceDTO>();
        foreach (Resource resource in resources)
            resourceDTOs.Add(new ResourceDTO
            {
                Name = resource.Name,
                Type = resource.Type,
                Description = resource.Description,
                ConcurrentUsage = resource.ConcurrentUsage,
                Id = resource.Id
            });

        return resourceDTOs;
    }
}