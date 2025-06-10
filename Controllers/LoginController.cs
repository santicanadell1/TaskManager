using DataAccess;
using Service;
using Service.Interface;

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
}