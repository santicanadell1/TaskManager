using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.LeaderPServiceException;
using Service.Interface;
using Service.Models;
using Task = Domain.Task;

namespace Service;

public class LeaderPService : ILeaderPService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly TaskService _taskService;
    private readonly AdminPService _adminPService;
    private readonly CpmService _cpmService;

    public LeaderPService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _cpmService = new CpmService();
        _taskService = new TaskService(repositoryManager, _cpmService);
        _adminPService = new AdminPService(repositoryManager);
    }

    public void UpdateTask(string projectName, string taskTitle, TaskDTO taskDTO)
    {
        CheckProjectLeaderRole(projectName);
        _taskService.UpdateTask(projectName, taskTitle, taskDTO);
    }

    public List<ProjectDTO> GetAllMyProjects()
    {
        CheckProjectLeaderRole();
        UserDTO currentUser = LoggedUser.Current;
        return _adminPService.GetAllProjectsForUser(currentUser.Email);
    }

    public ProjectDTO GetProject(string projectName)
    {
        CheckProjectLeaderRole(projectName);

        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null)
            throw new ProjectNotFoundException();

        ProjectConverter projectConverter = new ProjectConverter(_repositoryManager);
        return projectConverter.FromEntity(project);
    }

    //do not show resources from the ui because the leader can't edit the resources
    public TaskDTO GetTask(string projectName, string taskTitle)
    {
        CheckProjectLeaderRole(projectName);
        return _taskService.GetTask(projectName, taskTitle);
    }

    //do not show resources from the ui because the leader can't edit the resources
    public List<TaskDTO> GetTasks(string projectName)
    {
        CheckProjectLeaderRole(projectName);
        return _taskService.GetTasks(projectName);
    }


    public void AssignMembersToProject(string projectName, List<UserDTO> membersDTO)
    {
        UserDTO currentUser = LoggedUser.Current;
        bool isAdminProject = false;

        AddTempAdminProjectRole(currentUser, isAdminProject);

        _adminPService.AssignMembersToProject(projectName, membersDTO);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);
    }

    public List<UserDTO> GetAllMembersOfAProject(string projectName)
    {
        UserDTO currentUser = LoggedUser.Current;
        bool isAdminProject = false;

        AddTempAdminProjectRole(currentUser, isAdminProject);

        List<UserDTO> members = _adminPService.GetMembers(projectName);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);

        return members;
    }

    public void RemoveMemberFromProject(string projectName, string memberToRemoveEmail)
    {
        UserDTO currentUser = LoggedUser.Current;
        bool isAdminProject = false;

        AddTempAdminProjectRole(currentUser, isAdminProject);
        _adminPService.RemoveMemberFromProject(projectName, memberToRemoveEmail);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);
    }

    public List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string memberEmail)
    {
        UserDTO currentUser = LoggedUser.Current;
        bool isAdminProject = false;

        AddTempAdminProjectRole(currentUser, isAdminProject);
        List<TaskDTO> memberTasks = new List<TaskDTO>();
        memberTasks = _adminPService.GetAllTaskForAMemberInAProject(projectName, memberEmail);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);

        return memberTasks;
    }

    public void AddTaskToMember(string projectName, string memberEmail, string taskTitle)
    {
        UserDTO currentUser = LoggedUser.Current;
        bool isAdminProject = false;

        AddTempAdminProjectRole(currentUser, isAdminProject);

        _adminPService.AddTaskToMember(projectName, memberEmail, taskTitle);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);
    }

    public void RemoveTaskFromMember(string projectName, string memberEmail, string taskTitle)
    {
        UserDTO currentUser = LoggedUser.Current;
        bool isAdminProject = false;

        AddTempAdminProjectRole(currentUser, isAdminProject);

        _adminPService.RemoveTaskFromMember(projectName, memberEmail, taskTitle);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);
    }

    private void CheckProjectLeaderRole(string projectName)
    {
        UserDTO currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.ProjectLeader))
            throw new UnauthorizedLeaderAccessException();

        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        if (project.ProjectLeader == null || project.ProjectLeader.Email != currentUser.Email)
            throw new UnauthorizedLeaderAccessException();
    }

    private void AddTempAdminProjectRole(UserDTO user, bool isAdminProject = false)
    {
        if (user.Roles.Contains(RolDTO.AdminProject))
        {
            isAdminProject = true;
        }
        else
        {
            user.Roles.Add(RolDTO.AdminProject);
        }
    }

    private void RemoveTempAdminProjectRole(UserDTO user, bool isAdminProject)
    {
        if (!isAdminProject)
        {
            user.Roles.Remove(RolDTO.AdminProject);
        }
    }

    private void CheckProjectLeaderRole()
    {
        UserDTO currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.ProjectLeader))
            throw new UnauthorizedLeaderAccessException();
    }
}