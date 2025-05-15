using DataAccess;
using Domain.Exceptions.TaskExceptions;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Interface;
using Service.Exceptions.MemberServiceExceptions;
using Service.Models;


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
        if (!user.Roles.Contains(RolDTO.ProjectMember))
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
        TaskService taskService = new TaskService(_database, new CpmService());
        CheckIsTaskOfTheUser((int)task.Id, email);
        foreach (var previousTask in task.PreviousTasks)
        {
            TaskDTO previousTaskDTO = taskService.GetTask(projectName, (int)previousTask.Id);
            if (!CheckIfTaskIsCompleted(previousTaskDTO))
            {
                throw new TaskException("Task state can't be changed because it's previous tasks are not completed.");
            }
        }
        task.State = status;
        taskService.UpdateTask(projectName, task.Id, task);
    }

    private bool CheckIfTaskIsCompleted(TaskDTO task)
    {
        return task.State == StateDTO.DONE;
    }
    
    
}