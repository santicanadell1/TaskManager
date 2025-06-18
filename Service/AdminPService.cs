using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service;
using Service.Converter;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.LeaderPServiceException;
using Service.Interface;
using Service.Models;
using Task = Domain.Task;

public class AdminPService : IAdminPService
{
    private readonly ProjectConverter _projectConverter;
    private readonly IRepositoryManager _repositoryManager;
    private readonly UserConverter _userConverter;

    public AdminPService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _projectConverter = new ProjectConverter(_repositoryManager);
        _userConverter = new UserConverter(_repositoryManager);
    }

    public void CreateProject(ProjectDTO projectDTO)
    {
        CheckAdminProyectRole();
        var existingProject = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        if (existingProject != null) throw new DuplicatedProjectsNameException();
        if (projectDTO.StartDate < DateTime.Today)
            throw new ProjectStartDateException();
        var newProject = _projectConverter.ToEntity(projectDTO);
        SetProjectAdmin(newProject, projectDTO);
        _repositoryManager.ProjectRepository.Add(newProject);
    }

    public void AssignMembersToProject(string projectName, List<UserDTO> membersDTO)
    {
        CheckAdminProyectRole();
        try
        {
            if (string.IsNullOrEmpty(projectName)) throw new ArgumentException("Project name cannot be null or empty");
            if (membersDTO == null || !membersDTO.Any()) throw new ArgumentException("No members provided");

            CheckAdminProyectRole();

            var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
            if (project == null) throw new ProjectNotFoundException();


            if (project.Members == null) project.Members = new List<User>();

            foreach (var memberDTO in membersDTO)
            {
                if (project.Members.Any(u => u.Email == memberDTO.Email)) throw new UserIsAlreadyAMemberException();

                var existingUser = _repositoryManager.UserRepository.Get(u => u.Email == memberDTO.Email);
                if (existingUser == null) throw new UserNotFoundException();


                if (project.Members.Any(u => u.Id == existingUser.Id)) throw new UserIsAlreadyAMemberException();

                project.Members.Add(existingUser);
            }

            _repositoryManager.ProjectRepository.Update(project);
        }
        catch (ProjectNotFoundException pnfEx)
        {
            throw;
        }
        catch (UserNotFoundException unfEx)
        {
            throw;
        }
        catch (UserIsAlreadyAMemberException uamEx)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to assign members to project: {ex.Message}", ex);
        }
    }

    public void RemoveMemberFromProject(string projectName, string memberEmail)
    {
        CheckAdminProyectRole();
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        var user = _repositoryManager.UserRepository.Get(u => u.Email == memberEmail);
        if (project.Members.Find(m => m.Email == memberEmail) == null) throw new UserIsNotAMemberException();

        var projectTasks = project.Tasks.Select(t => t.Id.Value).ToHashSet();
        List<Task> TasksToDelete = user.Tasks
            .Where(t => t.Id.HasValue && projectTasks.Contains(t.Id.Value))
            .ToList();
        foreach (var task in TasksToDelete) user.Tasks.Remove(task);
        _repositoryManager.UserRepository.Update(user);

        project.Members.Remove(project.Members.Find(m => m.Email == memberEmail));
        _repositoryManager.ProjectRepository.Update(project);
    }

    public void RemoveProject(string projectName)
    {
        CheckAdminProyectRole();
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        _repositoryManager.ProjectRepository.Delete(project);
    }

    public void UpdateProject(string projectNameToUpdate, ProjectDTO updatedProjectDTO)
    {
        CheckAdminProyectRole();
        var existingProject = _repositoryManager.ProjectRepository.Get(p => p.Name == projectNameToUpdate);
        if (existingProject == null) throw new ProjectNotFoundException();

        var updatedProject = _projectConverter.ToEntity(updatedProjectDTO);
        updatedProject.Id = existingProject.Id;
        SetProjectAdmin(updatedProject, updatedProjectDTO);

        _repositoryManager.ProjectRepository.Update(updatedProject);
    }

    public List<ProjectDTO> GetAllProjects()
    {
        CheckAdminProyectRole();
        var projects = _repositoryManager.ProjectRepository.GetAll();
        List<ProjectDTO> projectDTOs = new List<ProjectDTO>();
        var loggedUser = LoggedUser.Current;
        foreach (var project in projects)
            if (project.AdminProject.Email == loggedUser.Email)
                projectDTOs.Add(_projectConverter.FromEntity(project));

        return projectDTOs;
    }

    public ProjectDTO GetProjectByName(string projectName)
    {
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        return _projectConverter.FromEntity(project);
    }

    public List<ProjectDTO> GetAllProjectsForUser(string Email)
    {
        List<ProjectDTO> projects = new List<ProjectDTO>();
        foreach (var project in _repositoryManager.ProjectRepository.GetAll())
            if ((project.AdminProject != null && project.AdminProject.Email == Email) ||
                (project.ProjectLeader != null && project.ProjectLeader.Email == Email) ||
                project.Members.Any(m => m.Email == Email))
                projects.Add(_projectConverter.FromEntity(project));

        return projects;
    }

    public List<ProjectDTO> GetAllProjectsForAdmin()
    {
        List<ProjectDTO> projects = new List<ProjectDTO>();
        foreach (var project in _repositoryManager.ProjectRepository.GetAll())
            if ((project.AdminProject != null && project.AdminProject.Email == LoggedUser.Current.Email) ||
                LoggedUser.Current.Roles.Equals(RolDTO.AdminSystem))
                projects.Add(_projectConverter.FromEntity(project));
        return projects;
    }

    public List<UserDTO> GetMembers(string projectName)
    {
        CheckAdminProyectRole();
        var project = GetProjectByName(projectName);
        if (project == null) throw new ProjectNotFoundException();

        var memberDTOs = project.Members;
        return memberDTOs;
    }

    public void AddTaskToMember(string projectName, string memberEmail, string title)
    {
        CheckAdminProyectRole();
        var projectEntity = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (projectEntity == null) throw new ProjectNotFoundException();

        if (projectEntity.Members == null || !projectEntity.Members.Any(m => m.Email == memberEmail))
            throw new UserIsNotAMemberException();

        if (projectEntity.Tasks == null || !projectEntity.Tasks.Any(t => t.Title == title))
            throw new TaskIsNotFromTheProjectException();

        var userEntity = _repositoryManager.UserRepository.Get(u => u.Email == memberEmail);
        if (userEntity == null) throw new UserNotFoundException();

        var task = _repositoryManager.TaskRepository.Get(t => t.Title == title);
        userEntity.Tasks.Add(task);
        _repositoryManager.UserRepository.Update(userEntity);
    }

    public void RemoveTaskFromMember(string projectName, string memberEmail, string title)
    {
        CheckAdminProyectRole();
        var projectEntity = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (projectEntity == null) throw new ProjectNotFoundException();

        if (projectEntity.Members == null || !projectEntity.Members.Any(m => m.Email == memberEmail))
            throw new UserIsNotAMemberException();

        if (projectEntity.Tasks == null || !projectEntity.Tasks.Any(t => t.Title == title))
            throw new TaskIsNotFromTheProjectException();

        var userEntity = _repositoryManager.UserRepository.Get(u => u.Email == memberEmail);
        if (userEntity == null) throw new UserNotFoundException();

        var taskToRemove = projectEntity.Tasks.Find(t => t.Title == title);
        userEntity.RemoveTask(taskToRemove);
        _repositoryManager.UserRepository.Update(userEntity);
    }

    public List<TaskDTO> GetAllTaskForAMember(string email)
    {
        var user = _repositoryManager.UserRepository.Get(u => u.Email == email);
        var cpmService = new CpmService();
        ITaskService taskService = new TaskService(_repositoryManager, cpmService);
        var returnList = new List<TaskDTO>();
        foreach (var project in _repositoryManager.ProjectRepository.GetAll())
        {
            List<TaskDTO> tasks = taskService.GetTasks(project.Name);
            foreach (var taskDTO in tasks)
                if (taskDTO.Id.HasValue && user.Tasks.Any(t => t.Id == taskDTO.Id))
                    returnList.Add(taskDTO);
        }

        return returnList;
    }

    public List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string email)
    {
        var user = _repositoryManager.UserRepository.Get(u => u.Email == email);
        if (user == null) throw new UserNotFoundException();
        if (user.Tasks == null) return new List<TaskDTO>();

        var cpmService = new CpmService();
        ITaskService taskService = new TaskService(_repositoryManager, cpmService);

        var returnList = new List<TaskDTO>();
        var tasks = taskService.GetTasks(projectName);
        foreach (var taskDTO in tasks)
            if (taskDTO.Id.HasValue && user.Tasks.Any(t => t.Id == taskDTO.Id))
                returnList.Add(taskDTO);

        return returnList;
    }

    public List<UserDTO> GetAllProjectLeaderUsers()
    {
        List<User> allUsers = _repositoryManager.UserRepository.GetAll();
        List<UserDTO> projectLeaders = new List<UserDTO>();

        foreach (var user in allUsers)
            if (user.Roles.Contains(Rol.ProjectLeader))
                projectLeaders.Add(_userConverter.FromEntity(user));

        return projectLeaders;
    }

    public void SetProjectLeader(string projectName, string LeaderEmail)
    {
        CheckProjectLeaderRole(LeaderEmail);

        var projectEntity = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (projectEntity == null) throw new ProjectNotFoundException();

        if (projectEntity.ProjectLeader != null) throw new TheProjectAlredyHasALeader();

        projectEntity.ProjectLeader = _repositoryManager.UserRepository.Get(u => u.Email == LeaderEmail);
        _repositoryManager.ProjectRepository.Update(projectEntity);
    }

    public void RemoveProjectLeader(string projectName)
    {
        CheckAdminProyectRole();

        var projectEntity = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (projectEntity == null) throw new ProjectNotFoundException();

        if (projectEntity.ProjectLeader != null)
            projectEntity.ProjectLeader = null;
        else
            throw new TheProjectDoesNotHaveAProjectLeader();

        _repositoryManager.ProjectRepository.Update(projectEntity);
    }

    public List<TaskDTO> GetTasks(ProjectDTO projectDTO)
    {
        CheckAdminProyectRole();

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        if (project == null) throw new ProjectNotFoundException();

        if (project.Tasks == null || !project.Tasks.Any()) return new List<TaskDTO>();

        var taskDTOs = new List<TaskDTO>();
        foreach (var task in project.Tasks)
            taskDTOs.Add(new TaskDTO
            {
                Title = task.Title,
                Description = task.Description,
                ExpectedStartDate = task.ExpectedStartDate,
                Duration = task.Duration,
                State = (StateDTO)task.State,
                Id = task.Id,
                IsCritical = task.IsCritical,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                LatestStart = task.LatestStart,
                LatestFinish = task.LatestFinish,
                Slack = task.Slack,
                PreviousTasks = new List<TaskDTO>(),
                SameTimeTasks = new List<TaskDTO>()
            });

        return taskDTOs;
    }

    public void RemoveAdminFromProject(string projectName, string adminEmail)
    {
        CheckAdminProyectRole();
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        if (project.AdminProject == null || project.AdminProject.Email != adminEmail)
            throw new UserIsNotAMemberException();

        project.AdminProject = null;
        _repositoryManager.ProjectRepository.Update(project);
    }

    private void CheckAdminProyectRole()
    {
        var currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.AdminProject))
            throw new UnauthorizedAdminAccessException();
    }

    private void SetProjectAdmin(Project project, ProjectDTO projectDTO)
    {
        if (projectDTO.AdminProyect != null)
        {
            var adminFromDb = _repositoryManager.UserRepository.Get(u => u.Email == projectDTO.AdminProyect.Email);
            project.AdminProject = adminFromDb;
        }
        else
        {
            var adminFromDb = _repositoryManager.UserRepository.Get(u => u.Email == LoggedUser.Current.Email);
            project.AdminProject = adminFromDb;
        }
    }

    private void CheckProjectLeaderRole(string LeaderEmail)
    {
        var leaderUser = _repositoryManager.UserRepository.Get(u => u.Email == LeaderEmail);
        if (leaderUser == null || !leaderUser.Roles.Contains(Rol.ProjectLeader))
            throw new UnauthorizedLeaderAccessException();
    }
}