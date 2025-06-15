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

    public void CreateProject(ProjectDTO project)
    {
        _adminPService.CreateProject(project);
    }

    public List<ProjectDTO> GetAllProjects()
    {
        return _adminPService.GetAllProjects();
    }

    public void AssignMembersToProject(string projectName, List<UserDTO> usersToAssign)
    {
        _adminPService.AssignMembersToProject(projectName, usersToAssign);
    }

    public List<UserDTO> GetAllMembersOfAProject(string projectName)
    {
        return _adminPService.GetMembers(projectName);
    }

    public void RemoveProject(string projectName)
    {
        _adminPService.RemoveProject(projectName);
    }

    public void RemoveMemberFromProject(string projectName, string userToRemoveEmail)
    {
        _adminPService.RemoveMemberFromProject(projectName, userToRemoveEmail);
    }

    public void AddTaskToMember(string projectName, string userToAddTaskEmail, string taskName)
    {
        _adminPService.AddTaskToMember(projectName, userToAddTaskEmail, taskName);
    }

    public void RemoveTaskFromMember(string projectName, string userToDeleteTaskEmail, string taskName)
    {
        _adminPService.RemoveTaskFromMember(projectName, userToDeleteTaskEmail, taskName);
    }

    public void UpdateProject(string projectNameToUpdate, ProjectDTO project)
    {
        _adminPService.UpdateProject(projectNameToUpdate, project);
    }

    public List<UserDTO> GetAllProjectLeaderUsers()
    {
        return _adminPService.GetAllProjectLeaderUsers();
    }

    public void SetProjectLeaderToProject(string projectName, string usersToAssignLeaderEmail)
    {
        _adminPService.SetProjectLeader(projectName, usersToAssignLeaderEmail);
    }

    public void RemoveProjectLeaderFromProject(string projectName)
    {
        _adminPService.RemoveProjectLeader(projectName);
    }

    public ProjectDTO GetProject(string projectName)
    {
        return _adminPService.GetProjectByName(projectName);
    }

    public List<ProjectDTO> GetAllProjectsForAdmin()
    {
        return _adminPService.GetAllProjectsForAdmin();
    }
}