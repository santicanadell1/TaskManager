using DataAccess;
using Domain;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class AdminSystemController
{
    private readonly IAdminSService _adminSService;
    public AdminSystemController(IRepositoryManager repositoryManager)
    {
        _adminSService = new AdminSService(repositoryManager);
    }

    public void CreateUser(UserDTO user)
    {
        _adminSService.CreateUser(user);
    }

    public void AssignRole(UserDTO user, RolDTO role)
    {
        _adminSService.AssignRole(user, role);
    }

    public void ChangeUserPassword(string userEmail, string oldPassword, string newPassword)
    {
        _adminSService.ChangePassword(userEmail, oldPassword, newPassword);
    }

    public void ChangeToDefaultPassword(string userEmail, string oldPassword)
    {
        _adminSService.ChangeToDefaultPassword(userEmail, oldPassword);
    }

    public void DeleteUser(UserDTO user)
    {
        _adminSService.DeleteUser(user);
    }
}