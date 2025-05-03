using DataAccess;
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

        private Resource ToEntity(ResourceDTO resourceDTO)
        {
            Resource resource = new Resource(resourceDTO.Name, 
                resourceDTO.Type, resourceDTO.Description);
            return resource;
        }
    }
}