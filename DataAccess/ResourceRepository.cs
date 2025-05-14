using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;

namespace DataAccess;

public class ResourceRepository
{
    private readonly List<Resource> _resources;
    private static int _nextId;

    public ResourceRepository()
    {
        _resources = new List<Resource>();
        _nextId = 1;
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

        resource.Id = _nextId++;
        _resources.Add(resource);
    }

    public Resource? Get(Func<Resource, bool> filter)
    {
        return _resources.FirstOrDefault(filter);
    }


    public void Update(int? idToUpdate, Resource updatedResource)
    {
        int index = _resources.FindIndex(resource => resource.Id == idToUpdate);

        if (index == -1)
        {
            throw new ResourceNotFoundException();
        }

        _resources[index] = updatedResource;
    }

    public void Delete(int? idToDelete)
    {
        var resource = _resources.FirstOrDefault(r => r.Id == idToDelete);

        if (resource == null)
        {
            throw new ResourceNotFoundException();
        }

        _resources.Remove(resource);
    }
}