using DataAccess;
using Service;
using Service.Models;

namespace Controllers;

public class MemberProjectController
{
    private readonly MemberPService _memberPService;

    public MemberProjectController(IRepositoryManager repositoryManager)
    {
        _memberPService = new MemberPService(repositoryManager);
    }

    public void ChangeTaskStatus(string projectName, string userEmail, TaskDTO taskToUpdate, StateDTO newState)
    {
        _memberPService.ChangeTaskStatus(projectName, userEmail, taskToUpdate, newState);
    }
}