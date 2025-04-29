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
}
    
    
