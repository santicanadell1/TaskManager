using DataAccess;
using Service;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class LeaderProjectController
{
    private readonly ILeaderPService _LeaderPServiceJSON;
    private readonly ILeaderPService _LeaderPServiceCSV;
    private readonly ILeaderPService _LeaderPService;

    public LeaderProjectController(IRepositoryManager repositoryManager)
    {
        CSVExporter csvExporter = new CSVExporter(repositoryManager);
        JSONExporter jsonExporter = new JSONExporter(repositoryManager);
        _LeaderPService = new LeaderPService(repositoryManager, csvExporter);
        _LeaderPServiceCSV = new LeaderPService(repositoryManager, csvExporter);
        _LeaderPServiceJSON = new LeaderPService(repositoryManager, jsonExporter);
    }

    public void AssignMembersToProject(string projectName, List<UserDTO> members)
    {
        _LeaderPService.AssignMembersToProject(projectName, members);
    }

    public List<ProjectDTO> GetAllMyProjects()
    {
        return _LeaderPService.GetAllMyProjects();
    }

    public List<UserDTO> GetAllMembersOfAProject(string projectName)
    {
        return _LeaderPService.GetAllMembersOfAProject(projectName);
    }

    public void RemoveMemberFromProject(string projectName, string memberEmail)
    {
        _LeaderPService.RemoveMemberFromProject(projectName, memberEmail);
    }

    public List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string memberEmail)
    {
        return _LeaderPService.GetAllTaskForAMemberInAProject(projectName, memberEmail);
    }

    public void AddTaskToMember(string projectName, string memberEmail, string taskTitle)
    {
        _LeaderPService.AddTaskToMember(projectName, memberEmail, taskTitle);
    }

    public void RemoveTaskFromMember(string projectName, string memberEmail, string taskTitle)
    {
        _LeaderPService.RemoveTaskFromMember(projectName, memberEmail, taskTitle);
    }

    public string ExportProjectsAsJSON()
    {
        return _LeaderPServiceJSON.ExportProjects();
    }

    public string ExportProjectsAsCSV()
    {
        return _LeaderPServiceCSV.ExportProjects();
    }
}