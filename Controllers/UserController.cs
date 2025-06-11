using DataAccess;
using Service;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class UserController
{
    private readonly IUserService _userService;

    public UserController(IRepositoryManager repositoryManager)
    {
        _userService = new UserService(repositoryManager);
    }

    public List<UserDTO> GetAllUsers()
    {
        return _userService.GetUsers();
    }

    public UserDTO GetUser(string userEmail)
    {
        return _userService.GetUser(userEmail);
    }
}