using DataAccess.ResourceRepositoryExceptions;
using Domain;

namespace DataAccess;

public class ResourceRepository
{
    private readonly List<Resource> _resources;

    public ResourceRepository()
    {
        _resources = new List<Resource>();
    }

    public List<Resource> GetAll()
    {
        return _resources.ToList();
    }

    public void AddResource(Resource resource)
    {
        if (resource == null)
        {
            throw new ResourceIsNullException();
        }

        _resources.Add(resource);
    }

    public Resource? Get(Func<Resource, bool> filter)
    {
        return _resources.FirstOrDefault(filter);
    }

    public void Update(string nameToUpdate, string typeToUpdate, string descriptionToUpdate, Resource resource)
    {
        int index = _resources.FindIndex(resource =>
            resource.Name == nameToUpdate && resource.Type == typeToUpdate &&
            resource.Description == descriptionToUpdate);

        if (index == -1)
        {
            throw new ResourceNotFoundException();
        }

        _resources[index] = resource;
    }

    public void Delete(string nameToDelete, string typeToDelete, string descriptionToDelete)
    {
        var resource = _resources.FirstOrDefault(n =>
            n.Name == nameToDelete && n.Type == typeToDelete && n.Description == descriptionToDelete);

        if (resource == null)
        {
            throw new ResourceNotFoundException();
        }

        _resources.Remove(resource);
    }
}