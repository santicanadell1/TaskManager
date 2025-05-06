using DataAccess;
using Domain;
using Domain.Exceptions;
using Service;
using Service.Models;

public class AdminSService
{
    private readonly InMemoryDatabase _database;
    private UserService _userService;
    private PasswordManager _passwordManager = new PasswordManager();

    public AdminSService(InMemoryDatabase database)
    {
        _database = database;
        _userService = new UserService(_database);
    }

    private void CheckAdminRole()
    {
        var currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(Rol.AdminSystem))
        {
            throw new UnauthorizedAdminAccessException();
        }
    }

    public void CreateUser(UserDTO userDTO)
    {
        CheckAdminRole();
        _userService.AddUser(userDTO);
    }

    public void DeleteUser(UserDTO userDTO)
    {
        CheckAdminRole();

        var user = _userService.GetUser(userDTO.Email);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        _database.users.Delete(user.Email);
    }

    public void ChangePassword(string email, string newPassword, string oldPassword)
    {
        CheckAdminRole();

        var user = _userService.GetUser(email);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        if (user.Password != _passwordManager.HashPassword(oldPassword))
        {
            throw new InvalidOldPasswordException();
        }

        if (_passwordManager.IsValidPassword(newPassword))
        {
            user.Password = _passwordManager.HashPassword(newPassword);
            _userService.UpdateUser(user);
        }
        else
        {
            throw new InvalidUserPasswordException();
        }
    }

    public void AssignRole(UserDTO userDTO, Rol role)
    {
        CheckAdminRole();

        var user = _userService.GetUser(userDTO.Email);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        user.Roles.Add(role);

        _userService.UpdateUser(user);
    }
}