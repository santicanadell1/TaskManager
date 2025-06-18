using DataAccess;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service;
using Service.Converter;
using Service.Converters;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.UserServiceExceptions;
using Service.Interface;
using Service.Models;

public class AdminSService : IAdminSService
{
    private readonly NotificationConverter _notificationConverter;
    private readonly IPasswordManager _passwordManager = new PasswordManager();
    private readonly IRepositoryManager _repositoryManager;
    private readonly RolConverter _rolConverter;
    private readonly UserConverter _userConverter;
    private readonly IUserService _userService;


    public AdminSService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _userService = new UserService(repositoryManager);
        _rolConverter = new RolConverter();
        _userConverter = new UserConverter(repositoryManager);
    }

    public void CreateUser(UserDTO userDTO)
    {
        CheckAdminRole();
        _userService.AddUser(userDTO);
    }

    public void DeleteUser(UserDTO userDTO)
    {
        CheckAdminRole();
        var currentUser = LoggedUser.Current;
        var isAdminProject = false;
        IAdminPService adminPService = new AdminPService(_repositoryManager);
        var user = _userService.GetUser(userDTO.Email);

        if (user == null) throw new UserNotFoundException();

        if (!currentUser.Roles.Contains(RolDTO.AdminProject))
            currentUser.Roles.Add(RolDTO.AdminProject);
        else
            isAdminProject = true;

        var projects = adminPService.GetAllProjectsForUser(userDTO.Email);
        foreach (var project in projects)
        {
            try
            {
                adminPService.RemoveMemberFromProject(project.Name, userDTO.Email);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            try
            {
                adminPService.RemoveAdminFromProject(project.Name, userDTO.Email);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        if (!isAdminProject) currentUser.Roles.Remove(RolDTO.AdminProject);

        var userEntity = _repositoryManager.UserRepository.Get(u => u.Email == userDTO.Email);
        _repositoryManager.UserRepository.Delete(userEntity);
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
        if (!user.Roles.Contains(role))
        {
            user.Roles.Add(role);
            UpdateUserRoles(user);
        }
    }

    public void RemoveRole(UserDTO userDTO, RolDTO role)
    {
        CheckAdminRole();
        var user = _userService.GetUser(userDTO.Email);
        var usersWithAdminSystemRole = _userService.GetUsers().Count(u => u.Roles.Contains(RolDTO.AdminSystem));
        if (user == null) throw new UserNotFoundException();
        if (user.Roles.Contains(role))
        {
            if (role == RolDTO.AdminSystem && usersWithAdminSystemRole == 1)
                throw new AdminSServiceException("There must be at least one admin system user");
            user.Roles.Remove(role);
            UpdateUserRoles(user);
        }
        else
        {
            throw new AdminSServiceException("The user does not have this role");
        }
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
            user = _userConverter.ToEntity(userDTO);
        }

        user.Roles = _rolConverter.ConvertToDomainRoles(userDTO.Roles);
        _repositoryManager.UserRepository.Update(user);
    }

    private void CheckAdminRole()
    {
        var currentUser = LoggedUser.Current;

        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.AdminSystem))
            throw new UnauthorizedAdminAccessException();
    }

    private void CheckIsCurrenUser(string email)
    {
        var currentUser = LoggedUser.Current;
        if (currentUser == null || currentUser.Email != email) throw new UnauthorizedAdminAccessException();
    }
}