using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using Domain;
using Service.Exceptions.LeaderPServiceException;
using Service.Interface;
using Service.Models;

namespace Service;

public class LeaderPService : ILeaderPService
{
    private readonly IAdminPService _adminPService;
    private readonly CpmService _cpmService;
    private readonly IExporter _exporter;
    private readonly ProjectConverter _projectConverter;
    private readonly IRepositoryManager _repositoryManager;
    private readonly ITaskService _taskService;

    public LeaderPService(IRepositoryManager repositoryManager, IExporter exporter)
    {
        _repositoryManager = repositoryManager;
        _cpmService = new CpmService();
        _taskService = new TaskService(repositoryManager, _cpmService);
        _adminPService = new AdminPService(repositoryManager);
        _exporter = exporter;
        _projectConverter = new ProjectConverter(repositoryManager);
    }

    public string ExportProjects()
    {
        try
        {
            List<ProjectDTO> projects = GetAllMyProjects();

            if (projects == null || !projects.Any())
                return _exporter.Export(new List<ProjectDTO>());

            return _exporter.Export(projects);
        }
        catch (Exception)
        {
            throw new UnableToExportProject();
        }
    }

    public void UpdateTask(string projectName, string taskTitle, TaskDTO taskDTO)
    {
        CheckProjectLeaderRole(projectName);
        _taskService.UpdateTask(projectName, taskTitle, taskDTO);
    }

    public List<ProjectDTO> GetAllMyProjects()
    {
        CheckProjectLeaderRole();
        var currentUser = LoggedUser.Current;

        List<ProjectDTO> projects = new List<ProjectDTO>();

        foreach (var project in _repositoryManager.ProjectRepository.GetAll())
            if (project.ProjectLeader != null && project.ProjectLeader.Email == currentUser.Email)
                projects.Add(_projectConverter.FromEntity(project));

        return projects;
    }

    public ProjectDTO GetProject(string projectName)
    {
        CheckProjectLeaderRole(projectName);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null)
            throw new ProjectNotFoundException();

        var projectConverter = new ProjectConverter(_repositoryManager);
        return projectConverter.FromEntity(project);
    }

    public TaskDTO GetTask(string projectName, string taskTitle)
    {
        CheckProjectLeaderRole(projectName);
        return _taskService.GetTask(projectName, taskTitle);
    }

    public List<TaskDTO> GetTasks(string projectName)
    {
        CheckProjectLeaderRole(projectName);
        return _taskService.GetTasks(projectName);
    }

    public void AssignMembersToProject(string projectName, List<UserDTO> membersDTO)
    {
        var currentUser = LoggedUser.Current;
        var isAdminProject = false;

        AddTempAdminProjectRole(currentUser, ref isAdminProject);

        _adminPService.AssignMembersToProject(projectName, membersDTO);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);
    }

    public List<UserDTO> GetAllMembersOfAProject(string projectName)
    {
        var currentUser = LoggedUser.Current;
        var isAdminProject = false;

        AddTempAdminProjectRole(currentUser, ref isAdminProject);

        List<UserDTO> members = _adminPService.GetMembers(projectName);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);

        return members;
    }

    public void RemoveMemberFromProject(string projectName, string memberToRemoveEmail)
    {
        var currentUser = LoggedUser.Current;
        var isAdminProject = false;

        AddTempAdminProjectRole(currentUser, ref isAdminProject);
        _adminPService.RemoveMemberFromProject(projectName, memberToRemoveEmail);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);
    }

    public List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string memberEmail)
    {
        var currentUser = LoggedUser.Current;
        var isAdminProject = false;

        AddTempAdminProjectRole(currentUser, ref isAdminProject);
        List<TaskDTO> memberTasks = _adminPService.GetAllTaskForAMemberInAProject(projectName, memberEmail);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);

        return memberTasks;
    }

    public void AddTaskToMember(string projectName, string memberEmail, string taskTitle)
    {
        var currentUser = LoggedUser.Current;
        var isAdminProject = false;

        AddTempAdminProjectRole(currentUser, ref isAdminProject);

        _adminPService.AddTaskToMember(projectName, memberEmail, taskTitle);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);
    }

    public void RemoveTaskFromMember(string projectName, string memberEmail, string taskTitle)
    {
        var currentUser = LoggedUser.Current;
        var isAdminProject = false;

        AddTempAdminProjectRole(currentUser, ref isAdminProject);

        _adminPService.RemoveTaskFromMember(projectName, memberEmail, taskTitle);

        RemoveTempAdminProjectRole(currentUser, isAdminProject);
    }

    private void CheckProjectLeaderRole(string projectName)
    {
        var currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.ProjectLeader))
            throw new UnauthorizedLeaderAccessException();

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        if (project.ProjectLeader == null || project.ProjectLeader.Email != currentUser.Email)
            throw new UnauthorizedLeaderAccessException();
    }

    private void AddTempAdminProjectRole(UserDTO user, ref bool isAdminProject)
    {
        if (user.Roles.Contains(RolDTO.AdminProject))
            isAdminProject = true;
        else
            user.Roles.Add(RolDTO.AdminProject);
    }

    private void RemoveTempAdminProjectRole(UserDTO user, bool isAdminProject)
    {
        if (!isAdminProject) user.Roles.Remove(RolDTO.AdminProject);
    }

    private void CheckProjectLeaderRole()
    {
        var currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.ProjectLeader))
            throw new UnauthorizedLeaderAccessException();
    }
}