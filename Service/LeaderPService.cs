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

        public LeaderPService(IRepositoryManager repositoryManager, TaskService taskService)
        {
            _repositoryManager = repositoryManager;
            _taskService = taskService;
        }
        
        public void UpdateTask(string projectName, string taskTitle, TaskDTO taskDTO)
        {
            CheckProjectLeaderRole(projectName);
            _taskService.UpdateTask(projectName, taskTitle, taskDTO);
        }
        
        public List<ProjectDTO> GetMyProjects()
        {
            CheckProjectLeaderRole();  

            UserDTO currentUser = LoggedUser.Current;
            List<ProjectDTO> myProjects = new List<ProjectDTO>();

            foreach (Project project in _repositoryManager.ProjectRepository.GetAll())
            {
                if (project.ProjectLeader != null && project.ProjectLeader.Email == currentUser.Email)
                {
                    ProjectConverter projectConverter = new ProjectConverter(_repositoryManager);
                    myProjects.Add(projectConverter.FromEntity(project));
                }
            }

            return myProjects;
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
        
        public TaskDTO GetTask(string projectName, string taskTitle)
        {
            CheckProjectLeaderRole(projectName);
            return _taskService.GetTask(projectName, taskTitle);
        }
        
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