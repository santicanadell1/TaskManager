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

        public ResourceDTO Get(string name, string type, string description)
        {
            Resource? resource =
                _database.resources.Get(r => r.Name == name && r.Type == type && r.Description == description);

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

        public void UpdateResource(ResourceDTO resourceDTOToUpdate, ResourceDTO updatedResourceDTO)
        {
            Resource? resourceToUpdate = GetResourceObject(resourceDTOToUpdate.Name, resourceDTOToUpdate.Type,
                resourceDTOToUpdate.Description);

            Resource updatedResource = ToEntity(updatedResourceDTO);

            _database.resources.Update(resourceDTOToUpdate.Name, resourceDTOToUpdate.Type,
                resourceDTOToUpdate.Description,
                updatedResource);
        }

        public void DeleteResource(string name, string type, string description)
        {
            _database.resources.Delete(name, type, description);
        }

        private Resource GetResourceObject(string name, string type, string description)
        {
            Resource? resource =
                _database.resources.Get(r => r.Name == name && r.Type == type && r.Description == description);

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