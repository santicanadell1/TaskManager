using Domain;
using Service.Models;

namespace Service.Interface
{
    public interface ILeaderPService
    {
  
        void UpdateTask(string projectName, string taskTitle, TaskDTO taskDTO);
  
        TaskDTO GetTask(string projectName, string taskTitle);
        
        List<TaskDTO> GetTasks(string projectName);
        
        List<ProjectDTO> GetMyProjects();
        
        ProjectDTO GetProject(string projectName);
        
     
    }
}