using DataAccess;
using Service;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class LeaderProjectController
{
    private readonly ILeaderPService _LeaderPService;

    public LeaderProjectController(IRepositoryManager repositoryManager)
    {
        _LeaderPService = new LeaderPService(repositoryManager);
    }

    public void AssignMembersToProject(string projectName, List<UserDTO> members)
    {
        _LeaderPService.
    }
}