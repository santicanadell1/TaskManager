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
}
    
    
