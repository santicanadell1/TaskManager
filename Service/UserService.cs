using DataAccess;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service.Converter;
using Service.Exceptions.UserServiceExceptions;
using Service.Interface;
using Service.Models;

namespace Service;

public class UserService : IUserService
{
    private readonly IPasswordManager _passwordManager = new PasswordManager();
    private readonly IRepositoryManager _repositoryManager;
    private readonly RolConverter _rolConverter;
    private readonly TaskConverter _taskConverter;
    private readonly UserConverter _userConverter;

    public UserService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _rolConverter = new RolConverter();
        _taskConverter = new TaskConverter(repositoryManager);
        _userConverter = new UserConverter(repositoryManager);
    }

    public void AddUser(UserDTO userDTO)
    {
        if (_repositoryManager.UserRepository.GetAll().Count == 0) userDTO.Roles.Add(RolDTO.AdminSystem);
        ValidateUserEmailAndPassword(userDTO);
        _repositoryManager.UserRepository.Add(_userConverter.ToEntity(userDTO));
    }

    public void UpdateUser(UserDTO userDTO)
    {
        User user;
        if (userDTO.Id.HasValue)
        {
            user = _repositoryManager.UserRepository.Get(u => u.Id == userDTO.Id);
            if (user == null) throw new UserNotFoundException();
        }
        else
        {
            user = GetUserObject(userDTO.Email);
        }

        user.FirstName = userDTO.FirstName;
        user.LastName = userDTO.LastName;
        user.Email = userDTO.Email;
        user.Roles = _rolConverter.ConvertToDomainRoles(userDTO.Roles);
        user.Birthday = userDTO.Birthday;
        user.Password = userDTO.Password;

        _repositoryManager.UserRepository.Update(user);
    }

    public List<UserDTO> GetUsers()
    {
        List<UserDTO> usersDTO = new List<UserDTO>();

        foreach (var user in _repositoryManager.UserRepository.GetAll())
            usersDTO.Add(_userConverter.FromEntity(user));

        if (usersDTO.Count == 0) throw new NoUsersFoundException();

        return usersDTO;
    }

    public UserDTO GetUser(string email)
    {
        var user = _repositoryManager.UserRepository.Get(user => user.Email == email);
        if (user == null) throw new UserNotFoundException();

        return _userConverter.FromEntity(user);
    }


    private void ValidateUserEmailAndPassword(UserDTO userDTO)
    {
        foreach (var user in _repositoryManager.UserRepository.GetAll())
            if (user.Email == userDTO.Email)
                throw new InvalidUserEmailException();

        if (!_passwordManager.IsValidPassword(userDTO.Password)) throw new InvalidUserPasswordException();
    }

    private User GetUserObject(string email)
    {
        var user = _repositoryManager.UserRepository.Get(user => user.Email == email);
        if (user == null) throw new UserNotFoundException();

        return user;
    }
}