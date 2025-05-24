using DataAccess;
using Domain;
using Service.Exceptions.LoginExceptions;
using Service.Interface;
using Service.Models;
using Service.Models.Exceptions;

namespace Service;

public class Login : ILogin
{
    private readonly AppDbContext _database;
    private readonly PasswordManager _passwordManager = new();

    public Login(AppDbContext database)
    {
        _database = database;
    }

    public UserDTO GetLoggedUser()
    {
        return LoggedUser.Current;
    }

    public void LoginUser(string email, string password)
    {
        var user = _database.users.Get(user => user.Email == email);
        if (user == null || !_passwordManager.VerifyPassword(password, user.Password))
            throw new InvalidLoginCredentialsException();

        LoggedUser.Current = FromEntity(user);
    }


    public void Logout()
    {
        LoggedUser.Current = null;
    }

    public bool IsAdminSystem()
    {
        return LoggedUser.Current?.Roles.Contains(ConvertToDTORole(Rol.AdminSystem)) ?? false;
    }

    public bool IsAdminProject()
    {
        return LoggedUser.Current?.Roles.Contains(ConvertToDTORole(Rol.AdminProject)) ?? false;
    }

    public bool IsProjectMember()
    {
        return LoggedUser.Current?.Roles.Contains(ConvertToDTORole(Rol.ProjectMember)) ?? false;
    }

    private static UserDTO FromEntity(User user)
    {
        return new UserDTO
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = ConvertToDTORoles(user.Roles),
            Password = user.Password,
            Birthday = user.Birthday
        };
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
                throw new ArgumentException("Invalid role");
        }
    }

    private static List<RolDTO> ConvertToDTORoles(List<Rol> roles)
    {
        var roleDTOs = new List<RolDTO>();

        foreach (var role in roles)
            switch (role)
            {
                case Rol.AdminSystem:
                    roleDTOs.Add(RolDTO.AdminSystem);
                    break;
                case Rol.ProjectMember:
                    roleDTOs.Add(RolDTO.ProjectMember);
                    break;
                case Rol.AdminProject:
                    roleDTOs.Add(RolDTO.AdminProject);
                    break;
                default:
                    throw new InvalidRolException();
            }

        return roleDTOs;
    }

    public void UpdateUser(UserDTO userDTO)
    {
        LoggedUser.Current = userDTO;
    }
}