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
        _resources.Add(resource);
    }

    public Resource? Get(Func<Resource, bool> filter)
    {
        return _resources.FirstOrDefault(filter);
    }
}