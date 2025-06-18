using DataAccess;
using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;
using Service.Models;

namespace Service.Converter;

public class ResourceConverter : IConverter<Resource, ResourceDTO>
{
    private readonly IRepositoryManager _repositoryManager;

    public ResourceConverter(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    public ResourceDTO FromEntity(Resource resource)
    {
        var dto = new ResourceDTO
        {
            Id = resource.Id,
            Name = resource.Name,
            Type = resource.Type,
            Description = resource.Description,
            ConcurrentUsage = resource.ConcurrentUsage
        };

        if (resource.Project != null)
            dto.Project = new ProjectDTO
            {
                Id = resource.Project.Id,
                Name = resource.Project.Name
            };

        return dto;
    }

    public Resource ToEntity(ResourceDTO resourceDTO)
    {
        return new Resource(resourceDTO.Name, resourceDTO.Type, resourceDTO.Description)
        {
            Id = resourceDTO.Id,
            ConcurrentUsage = resourceDTO.ConcurrentUsage,
            Project = resourceDTO.Project != null
                ? _repositoryManager.ProjectRepository.Get(p => p.Id == resourceDTO.Project.Id)
                : null
        };
    }

    public List<ResourceDTO> ConvertFromResourceEntityList(List<Resource> resources)
    {
        if (resources == null)
            return new List<ResourceDTO>();
        return resources
            .Select(r => FromEntity(r))
            .ToList();
    }

    public List<ResourceDTO> FromResourceEntityList(List<Resource> resources)
    {
        if (resources == null)
            return new List<ResourceDTO>();
        return resources
            .Select(r => FromEntity(r))
            .ToList();
    }

    public List<Resource> ConvertToResourceEntityList(List<ResourceDTO> resourceDTOs)
    {
        if (resourceDTOs == null) return new List<Resource>();

        var resources = new List<Resource>();
        foreach (var resourceDTO in resourceDTOs)
        {
            var existing = _repositoryManager.ResourceRepository.Get(r => r.Id == resourceDTO.Id);
            if (existing == null)
                throw new ResourceNotFoundException();
            resources.Add(existing);
        }

        return resources;
    }

    public List<Resource> ToResourceEntityList(List<ResourceDTO> resourceDTOs)
    {
        if (resourceDTOs == null) return new List<Resource>();

        var resources = new List<Resource>();
        foreach (var resourceDTO in resourceDTOs)
        {
            var existing = _repositoryManager.ResourceRepository.Get(r => r.Id == resourceDTO.Id);
            if (existing == null)
                throw new ResourceNotFoundException();
            resources.Add(existing);
        }

        return resources;
    }
}