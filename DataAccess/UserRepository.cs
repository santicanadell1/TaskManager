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
        return _users;
    }
    public void AddUser(User user)
    {
        _users.Add(user);
    }
}