using Service.Models;

namespace Service.Interface;
public interface IAdminSService
{
    // User management
    void CreateUser(UserDTO userDTO);
    void DeleteUser(UserDTO userDTO);

    // Password management
    void ChangePassword(string email, string newPassword, string oldPassword);

    // Role management
    void AssignRole(UserDTO userDTO, RolDTO role);
}
