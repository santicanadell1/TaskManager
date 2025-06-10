using DataAccess;
using Domain;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class AdminSystemController
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IAdminSService _adminSService;
    public AdminSystemController(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
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
}