using DataAccess;
using DataAccess.ProjectRepositoryExceptions;
using Domain;
using Domain.Exceptions;
using Service;
using Service.Models;
using Service.Models.Exceptions;


public class AdminPService : IAdminPService
{
    private readonly InMemoryDatabase _database;

    public AdminPService(InMemoryDatabase database)
    {
        _database = database;
    }

    private void CheckAdminProyectRole()
    {
        var currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(RolDTO.AdminProject))
        {
            throw new UnauthorizedAdminAccessException();
        }
    }

    public void CreateProject(ProjectDTO projectDTO)
    {
        CheckAdminProyectRole();
        var existingProject = _database.projects.GetProject(p => p.Name == projectDTO.Name);
        if (existingProject != null)
        {
            throw new DuplicatedProjectsNameException();
        }

        var newProject = ToEntity(projectDTO);
        newProject.AdminProject = ToEntity(LoggedUser.Current);

        _database.projects.AddProject(newProject);
    }


    public void AssignMembersToProject(string projectName, List<UserDTO> membersDTO)
    {
        CheckAdminProyectRole();
        var project = _database.projects.GetProject(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        foreach (var memberDTO in membersDTO)
        {
            var user = ToEntity(memberDTO);

            if (project.Members.Any(u => u.Email == user.Email))
            {
                throw new UserIsAlreadyAMemberException();
            }

            project.AddMember(user);
        }

        _database.projects.UpdateProject(projectName, project);
    }

    public void RemoveMemberFromProject(string projectName, string memberEmail)
    {
        CheckAdminProyectRole();
        var project = _database.projects.GetProject(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        if (project.Members.Find(m => m.Email == memberEmail) == null)
        {
            throw new UserIsNotAMemberException();
        }

        project.Members.Remove(project.Members.Find(m => m.Email == memberEmail));
        _database.projects.UpdateProject(projectName, project);
    }

    public void RemoveProject(string projectName)
    {
        CheckAdminProyectRole();
        _database.projects.RemoveProject(projectName);
    }

    public void UpdateProject(string projectNameToUpdate, ProjectDTO updatedProjectDTO)
    {
        CheckAdminProyectRole();
        _database.projects.UpdateProject(projectNameToUpdate, ToEntity(updatedProjectDTO));
    }

    public List<ProjectDTO> GetAllProjects()
    {
        CheckAdminProyectRole();
        List<Project> projects = _database.projects.GetAllProjects();
        List<ProjectDTO> projectDTOs = new List<ProjectDTO>();
        foreach (var project in projects)
        {
            projectDTOs.Add(FromEntity(project));
        }

        return projectDTOs;
    }

    public ProjectDTO GetProjectByName(string projectName)
    {
        CheckAdminProyectRole();
        var project = _database.projects.GetProject(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        return FromEntity(project);
    }

    public List<ProjectDTO> GetAllProjectsForUser(string Email)
    {
        List<ProjectDTO> projects = new List<ProjectDTO>();
        foreach (var project in _database.projects.GetAllProjects())
        {
            if (project.AdminProject.Email == Email || project.Members.Any(m => m.Email == Email))
            {
                projects.Add(FromEntity(project));
            }
        }
        return projects;
    }

    private ProjectDTO FromEntity(Project project)
    {
        List<UserDTO> memberDTOs = new List<UserDTO>();
        if (project.Members != null)
        {
            foreach (var member in project.Members)
            {
                memberDTOs.Add(FromEntity(member));
            }
        }

        return new ProjectDTO()
        {
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            Members = memberDTOs,
            AdminProyect = project.AdminProject != null ? FromEntity(project.AdminProject) : null
        };
    }


    public Project ToEntity(ProjectDTO projectDTO)
    {
        List<User> members = new List<User>();
        if (projectDTO.Members != null)
        {
            foreach (var memberDTO in projectDTO.Members)
            {
                members.Add(ToEntity(memberDTO));
            }
        }

        return new Project
        {
            Name = projectDTO.Name,
            Description = projectDTO.Description,
            StartDate = projectDTO.StartDate,
            AdminProject = projectDTO.AdminProyect != null ? ToEntity(projectDTO.AdminProyect) : null,
            Members = members,
        };
    }

    private User ToEntity(UserDTO userDTO)
    {
        return new User
        {
            FirstName = userDTO.FirstName,
            LastName = userDTO.LastName,
            Email = userDTO.Email,
            Birthday = userDTO.Birthday,
            Password = userDTO.Password,
            Roles = ConvertToDomainRoles(userDTO.Roles)
        };
    }


    private UserDTO FromEntity(User user)
    {
        return new UserDTO
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Birthday = user.Birthday,
            Password = user.Password,
            Roles = ConvertToDTORoles(user.Roles)
        };
    }

    public List<UserDTO> GetMembers(string projectName)
    {
        CheckAdminProyectRole();
        var project = this.GetProjectByName(projectName);
        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        List<UserDTO> memberDTOs = project.Members;

        return memberDTOs;
    }

    public void AddTaskToMember(string projectName, string memberEmail, int taskID)
    {
        CheckAdminProyectRole();
        var project = this.GetProjectByName(projectName);
        Project projectEntity = _database.projects.GetProject(p => p.Name == projectName);
        if (project.Members.Find(m => m.Email == memberEmail) == null)
        {
            throw new UserIsNotAMemberException();
        }

        if (projectEntity.Tasks.Find(m => m.Id == taskID) == null)
        {
            throw new TaskIsNotFromTheProjectException();
        }

        User userEntity = _database.users.Get(u => u.Email == memberEmail);
        userEntity.AddTask(taskID);
        _database.users.Update(memberEmail, userEntity);
    }

    public void RemoveTaskFromMember(string projectName, string memberEmail, int taskID)
    {
        CheckAdminProyectRole();
        var project = this.GetProjectByName(projectName);
        Project projectEntity = _database.projects.GetProject(p => p.Name == projectName);
        if (project.Members.Find(m => m.Email == memberEmail) == null)
        {
            throw new UserIsNotAMemberException();
        }

        if (projectEntity.Tasks.Find(m => m.Id == taskID) == null)
        {
            throw new TaskIsNotFromTheProjectException();
        }

        User userEntity = _database.users.Get(u => u.Email == memberEmail);
        userEntity.RemoveTask(taskID);
        _database.users.Update(memberEmail, userEntity);
    }

    public List<TaskDTO> GetAllTaskForAMember(string email)
    {
        User user = _database.users.Get(u => u.Email == email);
        CpmService cpmService = new CpmService();
        TaskService taskService = new TaskService(_database, cpmService);
        List<TaskDTO> returnList = new List<TaskDTO>();
        foreach (var project in _database.projects.GetAllProjects())
        {
            List<TaskDTO> tasks = taskService.GetTasks(project.Name);
            foreach (var task in tasks)
            {
                if (task.Id.HasValue && user.Tasks.Contains((int)task.Id))
                {
                    returnList.Add(task);
                }
            }
        }

        return returnList;
    }


    public List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string email)
    {
        User user = _database.users.Get(u => u.Email == email);
        if (user.Tasks == null)
        {
            return new List<TaskDTO>();
        }
        CpmService cpmService = new CpmService();
        TaskService taskService = new TaskService(_database, cpmService);
        List<TaskDTO> returnList = new List<TaskDTO>();
        List<TaskDTO> tasks = taskService.GetTasks(projectName);
        foreach (var task in tasks)
        {
            if (task.Id.HasValue && user.Tasks.Contains((int)task.Id))
            {
                returnList.Add(task);
            }
        }

        return returnList;
    }

    private List<Rol> ConvertToDomainRoles(List<RolDTO> roleDTOs)
    {
        var roles = new List<Rol>();

        foreach (var roleDTO in roleDTOs)
        {
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
        }

        return roles;
    }

    private List<RolDTO> ConvertToDTORoles(List<Rol> roles)
    {
        var roleDTOs = new List<RolDTO>();

        foreach (var role in roles)
        {
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
        }

        return roleDTOs;
    }

    private RolDTO ConvertToDTORole(Rol role)
    {
        switch (role)
        {
            case Rol.AdminSystem:
                return RolDTO.AdminSystem;
            case Rol.ProjectMember:
                return RolDTO.ProjectMember;
            case Rol.AdminProject:
                return RolDTO.AdminProject;
            default:
                throw new InvalidRolException();
        }
    }
}