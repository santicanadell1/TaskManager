using DataAccess;
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
        _userRepository.Add(ToEntity(user));
    }

    private void ValidateUserEmail(string email)
    {
        foreach(var user in _userRepository.GetAll)
    }
    
    
}