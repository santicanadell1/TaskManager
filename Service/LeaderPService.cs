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
        private readonly CpmService _cpmService;

        public LeaderPService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
            _cpmService = new CpmService();
            _taskService = new TaskService(_repositoryManager, _cpmService);
        }

        public void AddTask(string projectName, TaskDTO taskDTO)
        {
            CheckProjectLeaderRole(projectName);
            _taskService.AddTask(projectName, taskDTO);
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