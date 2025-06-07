using DataAccess;
using Domain.Exceptions.TaskExceptions;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.MemberServiceExceptions;
using Service.Interface;
using Service.Models;
using Domain;
using Service.Converters;
using Task = Domain.Task;

namespace Service;

public class MemberPService : IMemberPService
{
    private readonly AdminPService _adminPService;
    private readonly UserService _userService;
    private readonly TaskService _taskService;
    private readonly CpmService _cpmService;
    private readonly IRepositoryManager _repositoryManager;
    private readonly NotificationConverter _notificationConverter;

    public MemberPService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;

        _cpmService = new CpmService();
        _adminPService =
            new AdminPService(_repositoryManager, _notificationConverter);
        _userService = new UserService(_repositoryManager);
        _taskService = new TaskService(_repositoryManager,_cpmService,_notificationConverter );
    }

    public List<ProjectDTO> GetAllProjectsFromAMember(string email)
    {
        List<ProjectDTO> projectsFromMember = new List<ProjectDTO>();
        List<ProjectDTO> projects = _adminPService.GetAllProjects();

        foreach (ProjectDTO project in projects)
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
        CheckIsTaskOfTheUser(task.Id, email);

        TaskDTO currentTask = _taskService.GetTask(projectName, task.Title);

        if (currentTask.PreviousTasks != null && currentTask.PreviousTasks.Count > 0)
        {
            foreach (TaskDTO previousTask in currentTask.PreviousTasks)
            {
                TaskDTO previousTaskDTO = _taskService.GetTask(projectName, previousTask.Title);
                if (!CheckIfTaskIsCompleted(previousTaskDTO))
                    throw new TaskException(
                        "Task state can't be changed because it's previous tasks are not completed.");
            }
        }

        currentTask.State = status;
        _taskService.UpdateTask(projectName, currentTask.Title, currentTask);
    }

    private void CheckUserRole(string email)
    {
        UserDTO user = _userService.GetUser(email);
        if (!user.Roles.Contains(RolDTO.ProjectMember))
            throw new UserIsNotAMemberException();
    }

    private void CheckIsTaskOfTheUser(int? taskId, string email)
    {
        User user = _repositoryManager.UserRepository.Get(u => u.Email == email);
        if (user == null)
            throw new TaskCantBeModifiedByUserException();

        List<Task> tasks = user.Tasks ?? new List<Task>();
        if (tasks.Count == 0 || !tasks.Any(t => t.Id == taskId))
            throw new TaskCantBeModifiedByUserException();
    }

    private bool CheckIfTaskIsCompleted(TaskDTO task)
    {
        return task.State == StateDTO.DONE;
    }
}