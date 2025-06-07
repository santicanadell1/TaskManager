using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Interface;
using Service.Models;
using Task = System.Threading.Tasks.Task;

public class AdminPService : IAdminPService
{
    private readonly IRepositoryManager _repositoryManager;

    public AdminPService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    public void CreateProject(ProjectDTO projectDTO)
    {
        CheckAdminProyectRole();
        var existingProject = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        if (existingProject != null) throw new DuplicatedProjectsNameException();

        var newProject = ToEntity(projectDTO);
        SetProjectAdmin(newProject, projectDTO);

        _repositoryManager.ProjectRepository.Add(newProject);
    }


    public void AssignMembersToProject(string projectName, List<UserDTO> membersDTO)
    {
        CheckAdminProyectRole();
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        foreach (var memberDTO in membersDTO)
        {
            var user = ToEntity(memberDTO);

            if (project.Members.Any(u => u.Email == user.Email)) throw new UserIsAlreadyAMemberException();

            project.AddMember(user);
        }

        _repositoryManager.ProjectRepository.Update(project);
    }

    public void RemoveMemberFromProject(string projectName, string memberEmail)
    {
        CheckAdminProyectRole();
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        if (project.Members.Find(m => m.Email == memberEmail) == null) throw new UserIsNotAMemberException();

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

        var updatedProject = ToEntity(updatedProjectDTO);

        updatedProject.Id = existingProject.Id;
        SetProjectAdmin(updatedProject, updatedProjectDTO);

        _repositoryManager.ProjectRepository.Update(updatedProject);
    }

    public List<ProjectDTO> GetAllProjects()
    {
        CheckAdminProyectRole();
        var projects = _repositoryManager.ProjectRepository.GetAll();
        List<ProjectDTO> projectDTOs = new List<ProjectDTO>();
        foreach (var project in projects) projectDTOs.Add(FromEntity(project));

        return projectDTOs;
    }

    public ProjectDTO GetProjectByName(string projectName)
    {
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
        if (project == null) throw new ProjectNotFoundException();

        return FromEntity(project);
    }

    public List<ProjectDTO> GetAllProjectsForUser(string Email)
    {
        List<ProjectDTO> projects = new List<ProjectDTO>();
        foreach (var project in _repositoryManager.ProjectRepository.GetAll())
            if (project.AdminProject.Email == Email || project.Members.Any(m => m.Email == Email))
                projects.Add(FromEntity(project));

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

        var task = projectEntity.Tasks.Find(t => t.Title == title);
        userEntity.RemoveTask(task);
        _repositoryManager.ProjectRepository.Update(userEntity);
    }

    public List<TaskDTO> GetAllTaskForAMember(string email)
    {
        var user = _repositoryManager.UserRepository.Get(u => u.Email == email);
        var cpmService = new CpmService();
        var taskService = new TaskService(_projectRepository, _notificationRepository, _userRepository, cpmService,
            _taskRepository, _resourceRepository);
        var returnList = new List<TaskDTO>();
        foreach (var project in _repositoryManager.ProjectRepository.GetAll())
        {
            var tasks = taskService.GetTasks(project.Name);
            foreach (var task in tasks)
                if (task.Id.HasValue && user.Tasks.Any(t => t.Id == task.Id))
                    returnList.Add(task);
        }

        return returnList;
    }


    public List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string email)
    {
        var user = _repositoryManager.UserRepository.Get(u => u.Email == email);
        if (user.Tasks == null) return new List<TaskDTO>();
        var cpmService = new CpmService();
        var taskService = new TaskService(_projectRepository, _notificationRepository, _userRepository, cpmService,
            _taskRepository, _resourceRepository);
        var returnList = new List<TaskDTO>();
        var tasks = taskService.GetTasks(projectName);
        foreach (var task in tasks)
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
        var currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.AdminProject))
            throw new UnauthorizedAdminAccessException();
    }

    private ProjectDTO FromEntity(Project project)
    {
        List<UserDTO> memberDTOs = new List<UserDTO>();
        if (project.Members != null)
            foreach (var member in project.Members)
                memberDTOs.Add(FromEntity(member));

        return new ProjectDTO
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            Members = memberDTOs,
            AdminProyect = project.AdminProject != null ? FromEntity(project.AdminProject) : null
        };
    }


    private Project ToEntity(ProjectDTO projectDTO)
    {
        var project = new Project
        {
            Id = projectDTO.Id,
            Name = projectDTO.Name,
            Description = projectDTO.Description,
            StartDate = projectDTO.StartDate,
            Members = new List<User>()
        };

        if (projectDTO.Members != null)
        {
            foreach (var memberDTO in projectDTO.Members)
            {
                var memberFromDb = _repositoryManager.UserRepository.Get(u => u.Email == memberDTO.Email);
                if (memberFromDb != null)
                {
                    project.Members.Add(memberFromDb);
                }
            }
        }

        return project;
    }

    private User ToEntity(UserDTO userDTO)
    {
        var user = new User
        {
            FirstName = userDTO.FirstName,
            LastName = userDTO.LastName,
            Email = userDTO.Email,
            Birthday = userDTO.Birthday,
            Password = userDTO.Password,
            Roles = ConvertToDomainRoles(userDTO.Roles),
            Id = userDTO.Id
        };

        return user;
    }


    private UserDTO FromEntity(User user)
    {
        return new UserDTO
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Birthday = user.Birthday,
            Password = user.Password,
            Roles = ConvertToDTORoles(user.Roles)
        };
    }

    private List<Rol> ConvertToDomainRoles(List<RolDTO> roleDTOs)
    {
        var roles = new List<Rol>();

        foreach (var roleDTO in roleDTOs)
            switch (roleDTO)
            {
                case RolDTO.AdminSystem:
                    roles.Add(Rol.AdminSystem);
                    break;
                case RolDTO.ProjectMember:
                    roles.Add(Rol.ProjectMember);
                    break;
                case RolDTO.AdminProject:
                    roles.Add(Rol.AdminProject);
                    break;
            }

        return roles;
    }

    private List<RolDTO> ConvertToDTORoles(List<Rol> roles)
    {
        var roleDTOs = new List<RolDTO>();

        foreach (var role in roles)
            switch (role)
            {
                case Rol.AdminSystem:
                    roleDTOs.Add(RolDTO.AdminSystem);
                    break;
                case Rol.ProjectMember:
                    roleDTOs.Add(RolDTO.ProjectMember);
                    break;
                case Rol.AdminProject:
                    roleDTOs.Add(RolDTO.AdminProject);
                    break;
            }

        return roleDTOs;
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
            project.AdminProject = _repositoryManager.UserRepository.Get(u => u.Email == LoggedUser.Current.Email);
        }
    }
}