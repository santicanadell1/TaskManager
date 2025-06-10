using DataAccess;
using Service;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class SignUpController
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IUserService _userService;
    public SignUpController(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _userService = new UserService(repositoryManager);
    }

    public void SignUp(UserDTO user)
    {
        _userService.AddUser(user);
    }
    
}