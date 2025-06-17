using Service.Models;

namespace Service.Interface;

public interface IAdminSService
{
    void CreateUser(UserDTO userDTO);
    void DeleteUser(UserDTO userDTO);


    void ChangePassword(string email, string newPassword, string oldPassword);

    void ChangeCurrentUserPassword(string userEmail, string oldPassword, string newPassword);
    void AssignRole(UserDTO userDTO, RolDTO role);

    void RemoveRole(UserDTO userDTO, RolDTO role);
}