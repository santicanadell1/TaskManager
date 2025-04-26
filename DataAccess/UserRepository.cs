using Domain;
namespace DataAccess;

public class UserRepository
{
    private readonly List<User> _users;

    public UserRepository()
    {
        _users = new List<User>();
    }
}