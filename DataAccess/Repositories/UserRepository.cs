using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class UserRepository : IRepository<User>
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

    public void Add(User user)
    {
        if (user == null) throw new UserNotFoundException();
        validateDuplicateEmail(user.Email);

        try
        {
            _db.Set<User>().Add(user);
            _db.SaveChanges();
        }
        catch (Exception)
        {
            throw new UserNotFoundException();
        }
    }

    public User? Get(Func<User, bool> filter)
    {
        return _db.Set<User>()
            .Include(u => u.Notifications)
            .ThenInclude(n => n.Project)
            .Include(u => u.Tasks)
            .FirstOrDefault(filter);
    }

    public void Update(User updatedUser)
    {
        if (updatedUser == null) throw new UserNotFoundException();

        var existingUser = _db.Users.FirstOrDefault(u => u.Id == updatedUser.Id);

        if (existingUser == null) throw new UserNotFoundException();

        existingUser.FirstName = updatedUser.FirstName;
        existingUser.LastName = updatedUser.LastName;
        existingUser.Password = updatedUser.Password;
        existingUser.Roles = updatedUser.Roles;
        existingUser.Tasks = updatedUser.Tasks;
        existingUser.Notifications = updatedUser.Notifications;

        try
        {
            _db.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new UserNotFoundException();
        }
    }


    public void Delete(User user)
    {
        try
        {
            var existingUser = _db.Users
                .Include(u => u.Notifications)
                .Include(u => u.Tasks)
                .FirstOrDefault(u => u.Email == user.Email);

            if (existingUser == null)
                throw new UserNotFoundException();

            existingUser.Notifications.Clear();
            existingUser.Tasks.Clear();

            _db.Users.Remove(existingUser);
            _db.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception($"Error deleting user: {e.Message}", e);
        }
    }

    private void validateDuplicateEmail(string email)
    {
        if (_db.Set<User>().ToList().Any(u => u.Email == email)) throw new UserEmailIsDuplicatedException();
    }
}