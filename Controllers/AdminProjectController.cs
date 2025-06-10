using DataAccess;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class AdminProjectController
{
    private readonly IAdminPService _adminPService;

    public AdminProjectController(IRepositoryManager repositoryManager)
    {
        _adminPService = new AdminPService(repositoryManager);
    }

    public List<ProjectDTO> GetAllProjectsForUser(string userEmail)
    {
        return _adminPService.GetAllProjectsForUser(userEmail);
    }

    public List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string userEmail)
    {
        return _adminPService.GetAllTaskForAMemberInAProject(projectName, userEmail);
    }
    
}