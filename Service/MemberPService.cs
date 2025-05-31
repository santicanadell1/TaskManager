using DataAccess;
using Domain.Exceptions.TaskExceptions;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.MemberServiceExceptions;
using Service.Interface;
using Service.Models;
using Domain;
using Task = Domain.Task;

namespace Service;

public class MemberPService : IMemberPService
{
    private readonly UserRepository _userRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly NotificationRepository _notificationRepository;
    private readonly TaskRepository _taskRepository;

    public MemberPService(UserRepository userRepository, ProjectRepository _projectRepository,NotificationRepository notificationRepository)
    {
        _projectRepository = _projectRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
    }

    public List<ProjectDTO> GetAllProjectsFromAMember(string email)
    {
        var adminPService = new AdminPService(_userRepository,_projectRepository,_notificationRepository);
        var userService = new UserService(_userRepository);
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
        var taskService = new TaskService(_projectRepository, _notificationRepository, _userRepository,new CpmService());
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
        var UserService = new UserService(_userRepository);
        var user = UserService.GetUser(email);
        if (!user.Roles.Contains(RolDTO.ProjectMember)) throw new UserIsNotAMemberException();
    }

    private void CheckIsTaskOfTheUser(int taskId, string email)
    {
        var user = _userRepository.Get(u => u.Email == email);
        if (user == null) throw new TaskCantBeModifiedByUserException();

        var tasks = user.Tasks ?? new List<Task>();
        if (tasks.Count == 0 || !tasks.Any(t=> t.Id == taskId)) throw new TaskCantBeModifiedByUserException();
    }

    private bool CheckIfTaskIsCompleted(TaskDTO task)
    {
        return task.State == StateDTO.DONE;
    }
}