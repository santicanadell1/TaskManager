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
        _users.Add(user);
    }
    public User? Get(Func<User, bool> filter)
    {
        return _users.FirstOrDefault(filter);
    }

}