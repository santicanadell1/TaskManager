using DataAccess;
using Domain;
using Domain.Exceptions;
using Service.Interfaces;
using Service.MemberServiceException;
using Service.Models;
using Service.Models.Exceptions;

namespace Service;

public class MemberPService : IMemberPService
{
    private InMemoryDatabase _database;

    public MemberPService(InMemoryDatabase database)
    {
        _database = database;
    }

    public List<ProjectDTO> GetAllProjectsFromAMember(string email)
    {
        AdminPService adminPService = new AdminPService(_database);
        UserService userService = new UserService(_database);
        List<ProjectDTO> projectsFromMember = new List<ProjectDTO>();
        List<ProjectDTO> projects = adminPService.GetAllProjects();
        UserDTO user = userService.GetUser(email);
        CheckUserRole(email);
        foreach (var project in projects)
        {
            if (project.Members.Any(m => m.Email == user.Email))
            {
                projectsFromMember.Add(project);
            }
        }

        if (projectsFromMember.Count == 0)
        {
            throw new UserHasNoProjectsException();
        }

        return projectsFromMember;
    }

    private void CheckUserRole(string email)
    {
        UserService UserService = new UserService(_database);
        UserDTO user = UserService.GetUser(email);
        if (!user.Roles.Contains(ConvertToDTORole(Rol.ProjectMember)))
        {
            throw new UserIsNotAMemberException();
        }
    }

    private void CheckIsTaskOfTheUser(int taskId, string email)
    {
        var user = _database.users.Get(u => u.Email == email);
        if (user == null)
        {
            throw new TaskCantBeModifiedByUserException();
        }

        List<int> taskIds = user.Tasks ?? new List<int>();
        if (taskIds.Count == 0 || !taskIds.Contains(taskId))
        {
            throw new TaskCantBeModifiedByUserException();
        }
    }

    public void ChangeTaskStatus(string projectName, string email, TaskDTO task, StateDTO status)
    {
        CheckIsTaskOfTheUser((int)task.Id, email);
        CpmService cpmService = new CpmService();
        TaskService taskService = new TaskService(_database, cpmService);
        task.State = status;
        taskService.UpdateTask(projectName, task.Id, task);
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