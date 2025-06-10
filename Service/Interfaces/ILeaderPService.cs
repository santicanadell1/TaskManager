using Service.Models;

namespace Service.Interface
{
    public interface ILeaderPService
    {
        void AddTask(string projectName, TaskDTO taskDTO);
     
    }
}