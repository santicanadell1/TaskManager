using DataAccess;
using Domain.Exceptions.TaskExceptions;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.MemberServiceExceptions;
using Service.Interface;
using Service.Models;

namespace Service;

public class MemberPService : IMemberPService
{
    private readonly AppDbContext _database;

    public MemberPService(AppDbContext database)
    {
        _database = database;
    }

    public List<ProjectDTO> GetAllProjectsFromAMember(string email)
    {
        var adminPService = new AdminPService(_database);
        var userService = new UserService(_database);
        var projectsFromMember = new List<ProjectDTO>();
        var projects = adminPService.GetAllProjects();
        var user = userService.GetUser(email);
        CheckUserRole(email);
        foreach (var project in projects)
            if (project.Members.Any(m => m.Email == user.Email))
                projectsFromMember.Add(project);

        if (projectsFromMember.Count == 0) throw new UserHasNoProjectsException();

        return projectsFromMember;
    }

    public void ChangeTaskStatus(string projectName, string email, TaskDTO task, StateDTO status)
    {
        var taskService = new TaskService(_database, new CpmService());
        CheckIsTaskOfTheUser((int)task.Id, email);
        foreach (var previousTask in task.PreviousTasks)
        {
            var previousTaskDTO = taskService.GetTask(projectName, (int)previousTask.Id);
            if (!CheckIfTaskIsCompleted(previousTaskDTO))
                throw new TaskException("Task state can't be changed because it's previous tasks are not completed.");
        }

        task.State = status;
        taskService.UpdateTask(projectName, task.Id, task);
    }

    private void CheckUserRole(string email)
    {
        var UserService = new UserService(_database);
        var user = UserService.GetUser(email);
        if (!user.Roles.Contains(RolDTO.ProjectMember)) throw new UserIsNotAMemberException();
    }

    private void CheckIsTaskOfTheUser(int taskId, string email)
    {
        var user = _database.users.Get(u => u.Email == email);
        if (user == null) throw new TaskCantBeModifiedByUserException();

        var taskIds = user.Tasks ?? new List<int>();
        if (taskIds.Count == 0 || !taskIds.Contains(taskId)) throw new TaskCantBeModifiedByUserException();
    }

    private bool CheckIfTaskIsCompleted(TaskDTO task)
    {
        return task.State == StateDTO.DONE;
    }
}