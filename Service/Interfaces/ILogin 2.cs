using Service.Models;

namespace Service.Interfaces;

public interface ILogin
{
    UserDTO GetLoggedUser();
    void LoginUser(string email, string password);
    void Logout();
}