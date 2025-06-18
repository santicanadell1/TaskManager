using DataAccess;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class AdminSystemController
{
    private readonly IAdminSService _adminSService;
    private readonly IPasswordManager _passwordManager;

    public AdminSystemController(IRepositoryManager repositoryManager)
    {
        _adminSService = new AdminSService(repositoryManager);
        _passwordManager = new PasswordManager();
    }

    public void CreateUser(UserDTO user)
    {
        _adminSService.CreateUser(user);
    }

    public void DeleteUser(UserDTO user)
    {
        _adminSService.DeleteUser(user);
    }

    public void AssignRole(UserDTO user, RolDTO role)
    {
        _adminSService.AssignRole(user, role);
    }

    public void ChangeUserPassword(string userEmail, string oldPassword, string newPassword)
    {
        _adminSService.ChangePassword(userEmail, oldPassword, newPassword);
    }

    public void ChangeCurrentUserPassword(string userEmail, string oldPassword, string newPassword)
    {
        _adminSService.ChangeCurrentUserPassword(userEmail, oldPassword, newPassword);
    }

    public string GetDefaultPassword()
    {
        return _passwordManager.getDefaultPassword();
    }

    public void RemoveRole(UserDTO user, RolDTO role)
    {
        _adminSService.RemoveRole(user, role);
    }
}