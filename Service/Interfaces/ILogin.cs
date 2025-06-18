using Service.Models;

namespace Service.Interface;

public interface ILogin
{
    UserDTO GetLoggedUser();
    void LoginUser(string email, string password);
    void Logout();
    bool IsAdminSystem();
    bool IsAdminProject();
    bool IsProjectMember();
    bool IsProjectLeader();
    void UpdateUser(UserDTO userToUpdate);
}