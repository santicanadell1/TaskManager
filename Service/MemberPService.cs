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

    private readonly AdminPService _adminPService;
    private readonly UserService _userService;
    private readonly TaskService _taskService;
    private readonly CpmService _cpmService;

    public MemberPService(UserRepository userRepository, ProjectRepository projectRepository,
        NotificationRepository notificationRepository, TaskRepository taskRepository)
    {
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _notificationRepository = notificationRepository;
        _taskRepository = taskRepository;

        _cpmService = new CpmService();
        _adminPService =
            new AdminPService(_userRepository, _projectRepository, _notificationRepository, _taskRepository);
        _userService = new UserService(_userRepository);
        _taskService = new TaskService(_projectRepository, _notificationRepository, _userRepository, _cpmService,
            _taskRepository);
    }

    public List<ProjectDTO> GetAllProjectsFromAMember(string email)
    {
        if (!_userRepository.Get(u => u.Email == email).Roles.Contains(Rol.ProjectMember))
        {
            throw new UserIsNotAMemberException();
        }
        var projectsFromMember = new List<ProjectDTO>();
        var projects = _adminPService.GetAllProjects();

        foreach (var project in projects)
        {
            bool isProjectMember = project.Members != null && project.Members.Any(m => m.Email == email);
            bool isProjectAdmin = project.AdminProyect != null && project.AdminProyect.Email == email;

            if (isProjectMember || isProjectAdmin)
            {
                projectsFromMember.Add(project);
            }
        }

        if (projectsFromMember.Count == 0)
            throw new UserHasNoProjectsException();

        return projectsFromMember;
    }

    public void ChangeTaskStatus(string projectName, string email, TaskDTO task, StateDTO status)
    {
        CheckIsTaskOfTheUser((int)task.Id, email);

        var currentTask = _taskService.GetTask(projectName, (int)task.Id);

        if (currentTask.PreviousTasks != null && currentTask.PreviousTasks.Count > 0)
        {
            foreach (var previousTask in currentTask.PreviousTasks)
            {
                var previousTaskDTO = _taskService.GetTask(projectName, (int)previousTask.Id);
                if (!CheckIfTaskIsCompleted(previousTaskDTO))
                    throw new TaskException(
                        "Task state can't be changed because it's previous tasks are not completed.");
            }
        }

        currentTask.State = status;
        _taskService.UpdateTask(projectName, currentTask.Id, currentTask);
    }

    private void CheckUserRole(string email)
    {
        var user = _userService.GetUser(email);
        if (!user.Roles.Contains(RolDTO.ProjectMember))
            throw new UserIsNotAMemberException();
    }

    private void CheckIsTaskOfTheUser(int taskId, string email)
    {
        var user = _userRepository.Get(u => u.Email == email);
        if (user == null)
            throw new TaskCantBeModifiedByUserException();

        var tasks = user.Tasks ?? new List<Task>();
        if (tasks.Count == 0 || !tasks.Any(t => t.Id == taskId))
            throw new TaskCantBeModifiedByUserException();
    }

    private bool CheckIfTaskIsCompleted(TaskDTO task)
    {
        return task.State == StateDTO.DONE;
    }
}