using DataAccess;
using DataAccess.ResourceRepositoryExceptions;
using Domain;
using Domain.Exceptions;
using Service.Models;
using Task = System.Threading.Tasks.Task;

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

        public ResourceDTO Get(int? id)
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

        public void UpdateResource(int? id, ResourceDTO updatedResourceDTO)
        {
            Resource resourceToUpdate = GetResourceObject(id);

            Resource updatedResource = ToEntity(updatedResourceDTO);

            _database.resources.Update(id, updatedResource);
        }

        public void DeleteResource(int? id)
        {
            try
            {
                _database.resources.Delete(id);
            }
            catch (Exception ex)
            {
                throw new ResourceNotFoundException();
            }
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
                Description = resource.Description,
                Id = resource.Id,
            };
        }

        private Resource ToEntity(ResourceDTO resourceDTO)
        {
            var resource = new Resource(resourceDTO.Name, resourceDTO.Type, resourceDTO.Description)
            {
                Id = resourceDTO.Id
            };
            return resource;
        }
        
        private void CheckAdminRole(Resource resource)
        {
            var currentUser = LoggedUser.Current;
            List<Project> projects = GetProjectsThatAreUsingResource(resource);
            
            if (currentUser == null || !currentUser.Roles.Contains(Rol.AdminSystem) || CheckAdminProjectRole(resource))
            {
                throw new UnauthorizedAdminAccessException();
            }
        }
        
        private List<Project> GetProjectsThatAreUsingResource(Resource resource)
        {
            List<Project> projects = _database.projects.GetAllProjects();
            List<Project> projectsThatAreUsingResource = new List<Project>();
            foreach (var project in projects)
            {
                foreach (var task in project.Tasks)
                {
                    if (task.Resource.Contains(resource))
                    {
                        projectsThatAreUsingResource.Add(project);
                    }
                }
            }
            return projectsThatAreUsingResource;
        }

        private bool CheckAdminProjectRole(Resource resource)
        {
            var currentUser = LoggedUser.Current;
            List<Project> projects = GetProjectsThatAreUsingResource(resource);
            if (projects.Count == 0) return false;
            return currentUser.Roles.Contains(Rol.AdminProject) && (projects.Count == 1 &&
                   !(projects[0].AdminProject.Email.Equals(currentUser.Email)));
        }
    }
}