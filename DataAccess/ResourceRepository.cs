using DataAccess.ResourceRepositoryExceptions;
using Domain;

namespace DataAccess;

public class ResourceRepository
{
    private readonly List<Resource> _resources;
    private int _nextId;

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
        resource.SetId(_nextId++);
        _resources.Add(resource);
    }

    public Resource? Get(Func<Resource, bool> filter)
    {
        return _resources.FirstOrDefault(filter);
    }
    
    public void Update(int id, Resource resource)
    {
        var existingResource = _resources.FirstOrDefault(r = r.Id == id);
        if (existingResource == null)
        {
            throw new ResourceNotFoundException();
        }

        int index = _resources.FindIndex(r = r.Id == id);
        
        _resources[index] = resource;
    }
  
}