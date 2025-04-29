using DataAccess;
using Domain;
using Domain.Exceptions;
using Service.Models;

namespace Service;

public class UserService
{
    private readonly UserRepository _userRepository;

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public void AddUser(UserDTO userDTO)
    {
        ValidateUserEmail(userDTO.Email);
        _userRepository.AddUser(ToEntity(userDTO));
    }

    public void UpdateUser(UserDTO userDTO)
    {
        User? user = GetUserObject(userDTO.Email);
        user.FirstName = userDTO.FirstName;
        user.LastName = userDTO.LastName;
        user.Email = userDTO.Email;
        user.Roles = userDTO.Roles;
        user.Birthday = userDTO.Birthday;
        user.Password = userDTO.Password;
        _userRepository.Update(user.Email,user);
    }
    
    public List<UserDTO> GetUsers()
    {
        List<UserDTO> usersDTO = new List<UserDTO>();

        foreach (var user in _userRepository.GetAll())
        {
            usersDTO.Add(FromEntity(user));
        }

        if (usersDTO.Count == 0)
        {
            throw new NoUsersFoundException();
        }

        return usersDTO;
    }


    private void ValidateUserEmail(string email)
    {
        foreach (var user in _userRepository.GetAll())
        {
            if (user.Email == email)
            {
                throw new InvalidUserEmailException();
            }
        }
    }
    
    private static User ToEntity(UserDTO userDTO)
    {
        return new User
        {
            Email = userDTO.Email,
            FirstName = userDTO.FirstName,
            LastName = userDTO.LastName,
            Password = userDTO.Password,
            Birthday = userDTO.Birthday,
            Roles = userDTO.Roles
        };
    }
    private User GetUserObject(string email)
    {
        User? user = _userRepository.Get(user => user.Email == email);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        return user;
    }
    
    private static UserDTO FromEntity(User user)
    {
        return new UserDTO()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = user.Roles,
            Password = user.Password,
            Birthday = user.Birthday,
        };
    }
}
    
    
