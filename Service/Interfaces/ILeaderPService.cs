using Service.Models;

namespace Service.Interface
{
    public interface ILeaderPService
    {
        void AddTask(string projectName, TaskDTO taskDTO);

        void UpdateTask(string projectName, string taskTitle, TaskDTO taskDTO);
  
        TaskDTO GetTask(string projectName, string taskTitle);
        
        void DeleteTask(string projectName, string taskTitle);
        List<TaskDTO> GetTasks(string projectName);

        
        
     
    }
}