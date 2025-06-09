using DataAccess;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service;
using Service.Converters;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.UserServiceExceptions;
using Service.Interface;
using Service.Models;

public class AdminSService : IAdminSService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly PasswordManager _passwordManager = new();
    private readonly UserService _userService;
    private readonly NotificationConverter _notificationConverter;


    public AdminSService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _userService = new UserService(repositoryManager);
    }

    public void CreateUser(UserDTO userDTO)
    {
        CheckAdminRole();
        _userService.AddUser(userDTO);
    }

    public void DeleteUser(UserDTO userDTO)
    {
        CheckAdminRole();
        AdminPService adminPService = new AdminPService(_repositoryManager);
        UserDTO user = _userService.GetUser(userDTO.Email);

        if (user == null) throw new UserNotFoundException();
        List<ProjectDTO> projects = adminPService.GetAllProjectsForUser(userDTO.Email);
        foreach (ProjectDTO project in projects) adminPService.RemoveMemberFromProject(project.Name, userDTO.Email);
        User userEntity = _repositoryManager.UserRepository.Get(u => u.Email == userDTO.Email);
        _repositoryManager.UserRepository.Delete(userEntity);
    }

    public void ChangePassword(string email, string newPassword, string oldPassword)
    {
        CheckAdminRole();

        UserDTO user = _userService.GetUser(email);

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

        UserDTO user = _userService.GetUser(userDTO.Email);

        if (user == null) throw new UserNotFoundException();
        if (!user.Roles.Contains(role))
        {
            user.Roles.Add(role);
            _userService.UpdateUser(user);
        }
    }
    
    private void UpdateUserRoles(UserDTO userDTO)
    {
        User user;
        if (userDTO.Id.HasValue)
        {
            user = _repositoryManager.UserRepository.Get(u => u.Id == userDTO.Id);
            if (user == null) throw new UserNotFoundException();
        }
        else
        {
            user = (userDTO.Email);
        }

        user.Roles = _rolConverter.ConvertToDomainRoles(userDTO.Roles);
        _repositoryManager.UserRepository.Update(user);
    }

    private void CheckAdminRole()
    {
        UserDTO currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.AdminSystem))
            throw new UnauthorizedAdminAccessException();
    }

    public void ChangeToDefaultPassword(string email, string oldPassword)
    {
        CheckAdminRole();
        string defaultPassword = "Password123#";
        ChangePassword(email, defaultPassword, oldPassword);
    }

    public void ChangeCurrentUserPassword(string email, string oldPassword, string newPassword)
    {
        CheckIsCurrenUser(email);
        UserDTO user = _userService.GetUser(email);


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
        UserDTO currentUser = LoggedUser.Current;
        if (currentUser == null || currentUser.Email != email) throw new UnauthorizedAdminAccessException();
    }
}