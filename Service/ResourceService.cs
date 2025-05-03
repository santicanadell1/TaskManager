using DataAccess;
using DataAccess.ResourceRepositoryExceptions;
using Domain;
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
            Resource resource = new Resource(resourceDTO.Name, 
                resourceDTO.Type, resourceDTO.Description);
            return resource;
        }
    }
}