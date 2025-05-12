using DataAccess;
using Domain;
using Domain.Exceptions;
using Service.Exceptions;
using Service.Models;

namespace Service;

public class UserService : IUserService
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
        user.Roles = ConvertToDomainRoles(userDTO.Roles);
        user.Birthday = userDTO.Birthday;
        user.Password = userDTO.Password;
        user.Tasks = userDTO.Tasks;
        _database.users.Update(user.Email, user);
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
            Roles = ConvertToDomainRoles(userDTO.Roles),
            Tasks = userDTO.Tasks
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
            Roles = ConvertToDTORoles(user.Roles),
            Password = user.Password,
            Birthday = user.Birthday,
            Tasks = user.Tasks
        };
    }

    private List<Rol> ConvertToDomainRoles(List<RolDTO> roleDTOs)
    {
        var roles = new List<Rol>();

        foreach (var roleDTO in roleDTOs)
        {
            switch (roleDTO)
            {
                case RolDTO.AdminSystem:
                    roles.Add(Rol.AdminSystem);
                    break;
                case RolDTO.ProjectMember:
                    roles.Add(Rol.ProjectMember);
                    break;
                case RolDTO.AdminProject:
                    roles.Add(Rol.AdminProject);
                    break;
            }
        }

        return roles;
    }

    private List<RolDTO> ConvertToDTORoles(List<Rol> roles)
    {
        var roleDTOs = new List<RolDTO>();

        foreach (var role in roles)
        {
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
            }
        }

        return roleDTOs;
    }
}