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
}