using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class ResourceRepository
{
    private static int _nextId;
    protected readonly AppDbContext _db;

    public ResourceRepository(AppDbContext db)
    {
        _nextId = 1;
        _db = db;
    }

    public List<Resource> GetAll()
    {
        return _db.Set<Resource>().ToList();
    }

    public void AddResource(Resource resource)
    {
        if (resource == null) throw new ResourceIsNullException();
        try
        {
            _db.Set<Resource>().Add(resource);
            _db.SaveChanges();
            resource.Id = _nextId++;
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


    public void Update(int? idToUpdate, Resource updatedResource)
    {
        if (idToUpdate == null)
        {
            throw new ResourceNotFoundException();
        }

        if (updatedResource == null)
        {
            throw new ResourceNotFoundException();
        }

        var existingResource = _db.Resources.Find(idToUpdate);

        if (existingResource == null)
        {
            throw new ResourceNotFoundException();
        }

        try
        {
            _db.Entry(existingResource).CurrentValues.SetValues(updatedResource);
            _db.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new ResourceNotFoundException();
        }
    }


    public void Delete(int? idToDelete)
    {
        try
        {
            var existingResource = _db.Resources.Find(idToDelete);
            _db.Set<Resource>().Remove(existingResource);
            _db.SaveChanges();
        }
        catch (Exception e)
        {
            throw new ResourceNotFoundException();
        }
    }
}