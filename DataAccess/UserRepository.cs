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

    public void Update(string email, User user)
    {
        int index = _users.FindIndex(u => u.Email == email);
        if (index == -1)
        {
            throw new ArgumentException("User not found");
        }
        _users[index] = user;
    }

    public void Delete(string email)
    {
        int index = _users.FindIndex(u => u.Email == email);
        if (index == -1)
        {
            throw new ArgumentException("User not found");
        }
        _users.RemoveAt(index);
    }

}