using DataAccess;
using Domain;
using Domain.Exceptions;
using Service.Models;

namespace Service;

public class UserService
{
    private readonly InMemoryDatabase _database;
    private PasswordManager _passwordManager = new PasswordManager();
    public UserService(InMemoryDatabase database)
    {
        _database = database;
    }

    public void AddUser(UserDTO userDTO)
    {
        ValidateUserEmailAndPassword(userDTO);
        _database.users.AddUser(ToEntity(userDTO));
    }

    public void UpdateUser(UserDTO userDTO)
    {
        User? user = GetUserObject(userDTO.Email);
        user.FirstName = userDTO.FirstName;
        user.LastName = userDTO.LastName;
        user.Email = userDTO.Email;
        user.Roles = userDTO.Roles;
        user.Birthday = userDTO.Birthday;
        user.Password = _passwordManager.HashPassword(userDTO.Password);
        _database.users.Update(user.Email,user);
    }
    
    public List<UserDTO> GetUsers()
    {
        List<UserDTO> usersDTO = new List<UserDTO>();

        foreach (var user in _database.users.GetAll())
        {
            usersDTO.Add(FromEntity(user));
        }

        if (usersDTO.Count == 0)
        {
            throw new NoUsersFoundException();
        }

        return usersDTO;
    }
    
    public UserDTO GetUser(string email)
    {
        User? user = _database.users.Get(user => user.Email == email);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        return FromEntity(user);
    }


    private void ValidateUserEmailAndPassword(UserDTO userDTO)
    {
        foreach (var user in _database.users.GetAll())
        {
            if (user.Email == userDTO.Email)
            {
                throw new InvalidUserEmailException();
            }
        }
        
        if (!_passwordManager.IsValidPassword(userDTO.Password))
        {
            throw new InvalidUserPasswordException();
        }
    }
    
    private User ToEntity(UserDTO userDTO)
    {
        return new User
        {
            Email = userDTO.Email,
            FirstName = userDTO.FirstName,
            LastName = userDTO.LastName,
            Password = _passwordManager.HashPassword(userDTO.Password),
            Birthday = userDTO.Birthday,
            Roles = userDTO.Roles
        };
    }
    private User GetUserObject(string email)
    {
        User? user = _database.users.Get(user => user.Email == email);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        return user;
    }
    
    private UserDTO FromEntity(User user)
    {
        return new UserDTO()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = user.Roles,
            Password = user.Password,
            Birthday = user.Birthday,
        };
    }
}
    
    
