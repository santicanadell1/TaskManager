using System.Runtime.CompilerServices;
using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service;
using Service.Converter;
using Service.Converters;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Interface;
using Service.Models;
using Task = Domain.Task;

public class AdminPService : IAdminPService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ProjectConverter _projectConverter;
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
        Project existingProject = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        if (existingProject != null) throw new DuplicatedProjectsNameException();

        Project newProject = _projectConverter.ToEntity(projectDTO);
        SetProjectAdmin(newProject, projectDTO);

        _repositoryManager.ProjectRepository.Add(newProject);
    }


    public void AssignMembersToProject(string projectName, List<UserDTO> membersDTO)
    {
        CheckAdminProyectRole();
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        foreach (UserDTO memberDTO in membersDTO)
        {
            User user = _userConverter.ToEntity(memberDTO);

            if (project.Members.Any(u => u.Email == user.Email)) throw new UserIsAlreadyAMemberException();

            project.AddMember(user);
        }

        _repositoryManager.ProjectRepository.Update(project);
    }

    public void RemoveMemberFromProject(string projectName, string memberEmail)
    {
        CheckAdminProyectRole();
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        if (project.Members.Find(m => m.Email == memberEmail) == null) throw new UserIsNotAMemberException();

        project.Members.Remove(project.Members.Find(m => m.Email == memberEmail));
        _repositoryManager.ProjectRepository.Update(project);
    }

    public void RemoveProject(string projectName)
    {
        CheckAdminProyectRole();
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        _repositoryManager.ProjectRepository.Delete(project);
    }

    public void UpdateProject(string projectNameToUpdate, ProjectDTO updatedProjectDTO)
    {
        CheckAdminProyectRole();
        Project existingProject = _repositoryManager.ProjectRepository.Get(p => p.Name == projectNameToUpdate);
        if (existingProject == null) throw new ProjectNotFoundException();

        Project updatedProject = _projectConverter.ToEntity(updatedProjectDTO);

        updatedProject.Id = existingProject.Id;
        SetProjectAdmin(updatedProject, updatedProjectDTO);

        _repositoryManager.ProjectRepository.Update(updatedProject);
    }

    public List<ProjectDTO> GetAllProjects()
    {
        CheckAdminProyectRole();
        List<Project> projects = _repositoryManager.ProjectRepository.GetAll();
        List<ProjectDTO> projectDTOs = new List<ProjectDTO>();
        foreach (Project project in projects) projectDTOs.Add(_projectConverter.FromEntity(project));

        return projectDTOs;
    }

    public ProjectDTO GetProjectByName(string projectName)
    {
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        return _projectConverter.FromEntity(project);
    }

    public List<ProjectDTO> GetAllProjectsForUser(string Email)
    {
        List<ProjectDTO> projects = new List<ProjectDTO>();
        foreach (Project project in _repositoryManager.ProjectRepository.GetAll())
            if (project.AdminProject.Email == Email || project.Members.Any(m => m.Email == Email))
                projects.Add(_projectConverter.FromEntity(project));

        return projects;
    }

    public List<UserDTO> GetMembers(string projectName)
    {
        CheckAdminProyectRole();
        ProjectDTO project = GetProjectByName(projectName);
        if (project == null) throw new ProjectNotFoundException();

        List<UserDTO> memberDTOs = project.Members;

        return memberDTOs;
    }

    public void AddTaskToMember(string projectName, string memberEmail, string title)
    {
        CheckAdminProyectRole();
        Project projectEntity = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (projectEntity == null) throw new ProjectNotFoundException();

        if (projectEntity.Members == null || !projectEntity.Members.Any(m => m.Email == memberEmail))
            throw new UserIsNotAMemberException();

        if (projectEntity.Tasks == null || !projectEntity.Tasks.Any(t => t.Title == title))
            throw new TaskIsNotFromTheProjectException();

        User userEntity = _repositoryManager.UserRepository.Get(u => u.Email == memberEmail);
        if (userEntity == null) throw new UserNotFoundException();

        Task task = _repositoryManager.TaskRepository.Get(t => t.Title == title);
        userEntity.Tasks.Add(task);
        _repositoryManager.UserRepository.Update(userEntity);
    }

    public void RemoveTaskFromMember(string projectName, string memberEmail, string title)
    {
        CheckAdminProyectRole();
        Project projectEntity = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (projectEntity == null) throw new ProjectNotFoundException();

        if (projectEntity.Members == null || !projectEntity.Members.Any(m => m.Email == memberEmail))
            throw new UserIsNotAMemberException();

        if (projectEntity.Tasks == null || !projectEntity.Tasks.Any(t => t.Title == title))
            throw new TaskIsNotFromTheProjectException();

        User userEntity = _repositoryManager.UserRepository.Get(u => u.Email == memberEmail);
        if (userEntity == null) throw new UserNotFoundException();

        Task task = projectEntity.Tasks.Find(t => t.Title == title);
        userEntity.RemoveTask(task);
        _repositoryManager.UserRepository.Update(userEntity);
    }

    public List<TaskDTO> GetAllTaskForAMember(string email)
    {
        User user = _repositoryManager.UserRepository.Get(u => u.Email == email);
        CpmService cpmService = new CpmService();
        TaskService taskService = new TaskService(_repositoryManager, cpmService);
        List<TaskDTO> returnList = new List<TaskDTO>();
        foreach (Project project in _repositoryManager.ProjectRepository.GetAll())
        {
            List<TaskDTO> tasks = taskService.GetTasks(project.Name);
            foreach (TaskDTO task in tasks)
                if (task.Id.HasValue && user.Tasks.Any(t => t.Id == task.Id))
                    returnList.Add(task);
        }

        return returnList;
    }


    public List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string email)
    {
        User user = _repositoryManager.UserRepository.Get(u => u.Email == email);
        if (user.Tasks == null) return new List<TaskDTO>();

        CpmService cpmService = new CpmService();
        TaskService taskService =
            new TaskService(_repositoryManager, cpmService);

        List<TaskDTO> returnList = new List<TaskDTO>();
        List<TaskDTO> tasks = taskService.GetTasks(projectName);
        foreach (TaskDTO task in tasks)
        {
            if (task.Id.HasValue && user.Tasks.Any((t => t.Id == task.Id)))
            {
                returnList.Add(task);
            }
        }

        return returnList;
    }


    private void CheckAdminProyectRole()
    {
        UserDTO currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.AdminProject))
            throw new UnauthorizedAdminAccessException();
    }

    private void SetProjectAdmin(Project project, ProjectDTO projectDTO)
    {
        if (projectDTO.AdminProyect != null)
        {
            User adminFromDb = _repositoryManager.UserRepository.Get(u => u.Email == projectDTO.AdminProyect.Email);
            project.AdminProject = adminFromDb;
        }
        else
        {
            project.AdminProject = _repositoryManager.UserRepository.Get(u => u.Email == LoggedUser.Current.Email);
        }
    }
}