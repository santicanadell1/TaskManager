using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class UserRepository
{
    protected readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public List<User> GetAll()
    {
        return _db.Set<User>().ToList();
    }

    public void AddUser(User user)
    {
        if (user == null) throw new UserNotFoundException();
        try
        {
            _db.Set<User>().Add(user);
            _db.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new UserNotFoundException();
        }
    }

    public User? Get(Func<User, bool> filter)
    {
        return _db.Set<User>().FirstOrDefault(filter);
    }

    public void Update(string email, User user)
    {
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var existingUser = _db.Users.Find(email);

        if (existingUser == null)
        {
            throw new UserNotFoundException();
        }

        try
        {
            _db.Entry(existingUser).CurrentValues.SetValues(user);
            _db.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new UserNotFoundException();
        }
    }

    public void Delete(string email)
    {
        try
        {
            var existingUser = _db.Users.Find(email);
            _db.Set<User>().Remove(existingUser);
            _db.SaveChanges();
        }
        catch (Exception e)
        {
            throw new UserNotFoundException();
        }
    }
}