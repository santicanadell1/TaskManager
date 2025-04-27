using DataAccess;
using Domain;
using Service.Interfaces;
using Service.Models;

namespace Service;

public class Login : ILogin
{
    private readonly UserRepository _userRepository;

    public Login(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public UserDTO GetLoggedUser()
    {
        return LoggedUser.Current;
    }
    
    public void LoginUser(string email, string password)
    {
        User? user = _userRepository.Get(user => user.Email == email && user.Password == password);
        if (user == null)
        {
            throw new ArgumentException("User or password is incorrect, try again");
        }

        LoggedUser.Current = FromEntity(user);
    }
    
    public void Logout()
    {
        LoggedUser.Current = null;
    }
    
    private static UserDTO FromEntity(User user)
    {
        return new UserDTO()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = user.Roles,
            Password = user.Password,
            Birthday = user.Birthday
        };
    }
}