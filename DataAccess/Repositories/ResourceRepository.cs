using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class ResourceRepository : IRepository<Resource>
{
    protected readonly AppDbContext _db;

    public ResourceRepository(AppDbContext db)
    {
        _db = db;
    }

    public List<Resource> GetAll()
    {
        return _db.Set<Resource>().ToList();
    }

    public void Add(Resource resource)
    {
        if (resource == null) throw new ResourceIsNullException();
        try
        {
            _db.Set<Resource>().Add(resource);
            _db.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new ResourceIsNullException();
        }
    }

    public Resource? Get(Func<Resource, bool> filter)
    {
        return _db.Set<Resource>().FirstOrDefault(filter);
    }


    public void Update(Resource updatedResource)
    {
        if (updatedResource == null) throw new ResourceNotFoundException();

        var existingResource = _db.Resources.Find(updatedResource.Id);

        if (existingResource == null) throw new ResourceNotFoundException();

        existingResource.Name = updatedResource.Name;
        existingResource.Type = updatedResource.Type;
        existingResource.Description = updatedResource.Description;

        try
        {
            _db.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new ResourceNotFoundException();
        }
    }


    public void Delete(Resource resource)
    {
        try
        {
            var existingResource = _db.Resources.Find(resource.Id);
            _db.Set<Resource>().Remove(existingResource);
            _db.SaveChanges();
        }
        catch (Exception e)
        {
            throw new ResourceNotFoundException();
        }
    }
}