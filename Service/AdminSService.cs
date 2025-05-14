using DataAccess;
using Domain;
using Domain.Exceptions.NotificationExceptions;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Service;
using Service.Exceptions;
using Service.Models;
using Service.Models.Exceptions;

public class AdminSService : IAdminSService
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
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.AdminSystem))
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
        AdminPService adminPService = new AdminPService(_database);
        var user = _userService.GetUser(userDTO.Email);

        if (user == null)
        {
            throw new UserNotFoundException();
        }
        List<ProjectDTO> projects = adminPService.GetAllProjectsForUser(userDTO.Email);
        foreach (var project in projects)
        {
            adminPService.RemoveMemberFromProject(project.Name, userDTO.Email);
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

    public void AssignRole(UserDTO userDTO, RolDTO role)
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

    private RolDTO ConvertToDTORole(Rol role)
    {
        switch (role)
        {
            case Rol.AdminSystem:
                return RolDTO.AdminSystem;
            case Rol.ProjectMember:
                return RolDTO.ProjectMember;
            case Rol.AdminProject:
                return RolDTO.AdminProject;
            default:
                throw new InvalidRolException();
        }
    }
}