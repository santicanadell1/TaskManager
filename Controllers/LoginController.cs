using DataAccess;
using Service;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class LoginController
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ILogin _login;

    public LoginController(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _login = new Login(repositoryManager);
    }

    public void LoginUser(string email, string password)
    {
        _login.LoginUser(email, password);
    }

    public UserDTO GetLoggedUser()
    {
        return _login.GetLoggedUser();
    }

    public void UpdateLoggedUser(UserDTO userToUpdate)
    {
        _login.UpdateUser(userToUpdate);
    }

    public void Logout()
    {
        _login.Logout();
    }
}