using DataAccess;
using Domain;
using Service.Converter;
using Service.Exceptions.LoginExceptions;
using Service.Interface;
using Service.Models;

namespace Service;

public class Login : ILogin
{
    private readonly IPasswordManager _passwordManager = new PasswordManager();
    private readonly IRepositoryManager _repositoryManager;
    private readonly RolConverter _rolConverter;
    private readonly UserConverter _userConverter;

    public Login(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _rolConverter = new RolConverter();
        _userConverter = new UserConverter(_repositoryManager);
    }

    public UserDTO GetLoggedUser()
    {
        return LoggedUser.Current;
    }

    public void LoginUser(string email, string password)
    {
        var user = _repositoryManager.UserRepository.Get(user => user.Email == email);
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

    public bool IsProjectLeader()
    {
        return LoggedUser.Current?.Roles.Contains(_rolConverter.ConvertToDTORole(Rol.ProjectLeader)) ?? false;
    }

    public void UpdateUser(UserDTO userDTO)
    {
        LoggedUser.Current = userDTO;
    }
}