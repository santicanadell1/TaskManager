using DataAccess;
using Domain;
using Service.Converter;
using Service.Exceptions.LoginExceptions;
using Service.Interface;
using Service.Models;
using Service.Models.Exceptions;

namespace Service;

public class Login : ILogin
{
    private readonly PasswordManager _passwordManager = new();
    private readonly IRepositoryManager _repositoryManager;
    private readonly UserConverter _userConverter;
    private readonly RolConverter _rolConverter;
    private readonly TaskConverter _taskConverter;
    private readonly ResourceConverter _resourceConverter;

    public Login(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _resourceConverter = new ResourceConverter(repositoryManager);
        _rolConverter = new RolConverter();
        _taskConverter = new TaskConverter(_repositoryManager, _resourceConverter);
        _userConverter = new UserConverter(_repositoryManager, _rolConverter, _taskConverter);
    }

    public UserDTO GetLoggedUser()
    {
        return LoggedUser.Current;
    }

    public void LoginUser(string email, string password)
    {
        User user = _repositoryManager.UserRepository.Get(user => user.Email == email);
        if (user == null || !_passwordManager.VerifyPassword(password, user.Password))
            throw new InvalidLoginCredentialsException();

        LoggedUser.Current = _userConverter.FromEntity(user);
    }


    public void Logout()
    {
        LoggedUser.Current = null;
    }

    public bool IsAdminSystem()
    {
        return LoggedUser.Current?.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.AdminSystem)) ?? false;
    }

    public bool IsAdminProject()
    {
        return LoggedUser.Current?.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.AdminProject)) ?? false;
    }

    public bool IsProjectMember()
    {
        return LoggedUser.Current?.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.ProjectMember)) ?? false;
    }

    public void UpdateUser(UserDTO userDTO)
    {
        LoggedUser.Current = userDTO;
    }
}