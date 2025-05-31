using DataAccess;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Service;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.UserServiceExceptions;
using Service.Interface;
using Service.Models;

public class AdminSService : IAdminSService
{
    private readonly UserRepository _userRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly PasswordManager _passwordManager = new();
    private readonly UserService _userService;
    private readonly NotificationRepository _notificationRepository;
    private readonly TaskRepository _taskRepository;
    private readonly ResourceRepository _resourceRepository;
    

public AdminSService(UserRepository userRepository, ProjectRepository projectRepository, TaskRepository taskRepository, ResourceRepository resourceRepository)
    {
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _userService = new UserService(_userRepository);
        _taskRepository = taskRepository;
        _resourceRepository = resourceRepository;
    }

    public void CreateUser(UserDTO userDTO)
    {
        CheckAdminRole();
        _userService.AddUser(userDTO);
    }

    public void DeleteUser(UserDTO userDTO)
    {
        CheckAdminRole();
        var adminPService = new AdminPService(_userRepository,_projectRepository,_notificationRepository, _taskRepository, _resourceRepository);
        var user = _userService.GetUser(userDTO.Email);

        if (user == null) throw new UserNotFoundException();
        var projects = adminPService.GetAllProjectsForUser(userDTO.Email);
        foreach (var project in projects) adminPService.RemoveMemberFromProject(project.Name, userDTO.Email);
        _userRepository.Delete(user.Email);
    }

    public void ChangePassword(string email, string newPassword, string oldPassword)
    {
        CheckAdminRole();

        var user = _userService.GetUser(email);

        if (user == null) throw new UserNotFoundException();

        if (user.Password != _passwordManager.HashPassword(oldPassword)) throw new InvalidOldPasswordException();

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

        if (user == null) throw new UserNotFoundException();

        user.Roles.Add(role);

        _userService.UpdateUser(user);
    }

    private void CheckAdminRole()
    {
        var currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.AdminSystem))
            throw new UnauthorizedAdminAccessException();
    }

    public void ChangeToDefaultPassword(string email, string oldPassword)
    {
        CheckAdminRole();
        var defaultPassword = "Password123#";
        ChangePassword(email, defaultPassword, oldPassword);
    }

    public void ChangeCurrentUserPassword(string email, string oldPassword, string newPassword)
    {
        CheckIsCurrenUser(email);
        var user = _userService.GetUser(email);


        if (user == null) throw new UserNotFoundException();

        if (user.Password != _passwordManager.HashPassword(oldPassword)) throw new InvalidOldPasswordException();

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

    private void CheckIsCurrenUser(string email)
    {
        var currentUser = LoggedUser.Current;
        if (currentUser == null || currentUser.Email != email) throw new UnauthorizedAdminAccessException();
    }
}