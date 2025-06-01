using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class ResourceRepository
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

    public void AddResource(Resource resource)
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


    public void Update(int? idToUpdate, Resource updatedResource)
    {
        if (idToUpdate == null || updatedResource == null)
        {
            throw new ResourceNotFoundException();
        }

        var existingResource = _db.Resources.Find(idToUpdate);

        if (existingResource == null)
        {
            throw new ResourceNotFoundException();
        }

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