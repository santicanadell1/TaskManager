using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.LeaderPServiceException;
using Service.Interface;
using Service.Models;
using Task = Domain.Task;

namespace Service
{
    public class LeaderPService : ILeaderPService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly TaskService _taskService;
        private readonly AdminPService _adminPService;
        private readonly CpmService _cpmService;

        public LeaderPService(IRepositoryManager repositoryManager, TaskService taskService)
        {
            _repositoryManager = repositoryManager;
            _cpmService = new CpmService();
            _taskService = new TaskService(repositoryManager, _cpmService);
            _adminPService = new AdminPService(repositoryManager);
        }

        public void UpdateTask(string projectName, string taskTitle, TaskDTO taskDTO)
        {
            CheckProjectLeaderRole(projectName);
            _taskService.UpdateTask(projectName, taskTitle, taskDTO);
        }

        public List<ProjectDTO> GetAllMyProjects()
        {
            CheckProjectLeaderRole();
            UserDTO currentUser = LoggedUser.Current;
            return _adminPService.GetAllProjectsForUser(currentUser.Email);
        }

        public ProjectDTO GetProject(string projectName)
        {
            CheckProjectLeaderRole(projectName);

            Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
            if (project == null)
                throw new ProjectNotFoundException();

            ProjectConverter projectConverter = new ProjectConverter(_repositoryManager);
            return projectConverter.FromEntity(project);
        }

        //do not show resources from the ui
        public TaskDTO GetTask(string projectName, string taskTitle)
        {
            CheckProjectLeaderRole(projectName);
            return _taskService.GetTask(projectName, taskTitle);
        }

        //do not show resources from the ui
        public List<TaskDTO> GetTasks(string projectName)
        {
            CheckProjectLeaderRole(projectName);
            return _taskService.GetTasks(projectName);
        }

        private void CheckProjectLeaderRole()
        {
            UserDTO currentUser = LoggedUser.Current;
            if (currentUser == null || !currentUser.Roles.Contains(RolDTO.ProjectLeader))
                throw new UnauthorizedLeaderAccessException();
        }

        private void CheckProjectLeaderRole(string projectName)
        {
            UserDTO currentUser = LoggedUser.Current;
            if (currentUser == null || !currentUser.Roles.Contains(RolDTO.ProjectLeader))
                throw new UnauthorizedLeaderAccessException();

            Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);
            if (project == null) throw new ProjectNotFoundException();

            if (project.ProjectLeader == null || project.ProjectLeader.Email != currentUser.Email)
                throw new UnauthorizedLeaderAccessException();
        }
    }
}