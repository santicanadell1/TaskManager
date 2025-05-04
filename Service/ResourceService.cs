using DataAccess;
using DataAccess.ResourceRepositoryExceptions;
using Domain;
using Domain.Exceptions;
using Service.Models;

namespace Service
{
    public class ResourceService
    {
        private readonly InMemoryDatabase _database;

        public ResourceService(InMemoryDatabase database)
        {
            _database = database;
        }

        public void AddResource(ResourceDTO resourceDTO)
        {
            var resource = ToEntity(resourceDTO);
            _database.resources.AddResource(resource);
        }
        
        public ResourceDTO Get(int id)
        {
            Resource? resource = _database.resources.Get(r => r.Id == id);

            if (resource == null)
            {
                throw new ResourceNotFoundException();
            }
            
            return FromEntity(resource);
        }
        
        public List<ResourceDTO> GetResources()
        {
            List<ResourceDTO> resourcesDTO = new List<ResourceDTO>();

            foreach (var resource in _database.resources.GetAll())
            {
                resourcesDTO.Add(FromEntity(resource));
            }

            if (resourcesDTO.Count == 0)
            {
                throw new NoResourcesFoundException();
            }

            return resourcesDTO;
        }
        
        public void UpdateResource(ResourceDTO resourceDTO)
        {
            Resource? resource = GetResourceObject(resourceDTO.Id); 
            resource.Name = resourceDTO.Name;
            resource.Type = resourceDTO.Type;
            resource.Description = resourceDTO.Description;
            _database.resources.Update(resource.Id, resource);
        }
        
        private Resource GetResourceObject(int? id)
        {
            Resource? resource = _database.resources.Get(r => r.Id == id);
            if (resource == null)
            {
                throw new ResourceNotFoundException();
            }

            return resource;
        }

        
        private ResourceDTO FromEntity(Resource resource)
        {
            return new ResourceDTO()
            {
                Name = resource.Name,
                Type = resource.Type,
                Description = resource.Description
            };
        }

        private Resource ToEntity(ResourceDTO resourceDTO)
        {
            var resource = new Resource(resourceDTO.Name, 
                resourceDTO.Type, resourceDTO.Description);
            return resource;
        }
    }
}