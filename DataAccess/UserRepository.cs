using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;

namespace DataAccess;

public class UserRepository
{
    private readonly List<User> _users;

    public UserRepository()
    {
        _users = new List<User>();
    }

    public List<User> GetAll()
    {
        return _users.ToList();
    }

    public void AddUser(User user)
    {
        if (_users.Any(u => u.Email == user.Email)) throw new UserEmailIsDuplicatedException();
        _users.Add(user);
    }

    public User? Get(Func<User, bool> filter)
    {
        return _users.FirstOrDefault(filter);
    }

    public void Update(string email, User user)
    {
        if (!_users.Any(u => u.Email == email)) throw new UserNotFoundException();
        if (_users.Any(u => u.Email == user.Email) && user.Email != email) throw new UserEmailIsDuplicatedException();
        var index = _users.FindIndex(u => u.Email == email);

        _users[index] = user;
    }

    public void Delete(string email)
    {
        var index = _users.FindIndex(u => u.Email == email);
        if (index == -1) throw new UserNotFoundException();
        _users.RemoveAt(index);
    }
}