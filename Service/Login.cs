using DataAccess;
using Domain;
using Service.Exceptions;
using Service.Interfaces;
using Service.Models;

namespace Service;

public class Login : ILogin
{
    private readonly InMemoryDatabase _database;
    private PasswordManager _passwordManager = new PasswordManager();

    public Login(InMemoryDatabase database)
    {
        _database = database;
    }

    public UserDTO GetLoggedUser()
    {
        return LoggedUser.Current;
    }
    
    public void LoginUser(string email, string password)
    {
        User? user = _database.users.Get(user => user.Email == email);
        if (user == null || !_passwordManager.VerifyPassword(password, user.Password))
        {
            throw new InvalidLoginCredentialsException();
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