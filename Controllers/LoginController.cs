using DataAccess;
using Service;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class LoginController
{
    private readonly ILogin _login;

    public LoginController(IRepositoryManager repositoryManager)
    {
        _login = new Login(repositoryManager);
    }

    public void LoginUser(string email, string password)
    {
        _login.LoginUser(email, password);
    }

    public void Logout()
    {
        _login.Logout();
    }

    public UserDTO GetLoggedUser()
    {
        return _login.GetLoggedUser();
    }

    public void UpdateLoggedUser(UserDTO userToUpdate)
    {
        _login.UpdateUser(userToUpdate);
    }


    public bool IsTheCurrentUserAdminSystem()
    {
        return _login.IsAdminSystem();
    }

    public bool IsTheCurrentUserAdminProject()
    {
        return _login.IsAdminProject();
    }

    public bool IsTheCurrentUserProjectMember()
    {
        return _login.IsProjectMember();
    }

    public bool IsTheCurrentUserLeaderProject()
    {
        return _login.IsProjectLeader();
    }
}