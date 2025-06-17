using Service.Models;

namespace Service.Interface;

public interface ITaskService
{
    TaskDTO GetTask(string title);
    List<TaskDTO> GetTasks();
    TaskDTO GetTask(string projectName, string title);
    List<TaskDTO> GetTasks(string projectName);
    void AddTask(string projectName, TaskDTO taskDTO, bool solve = false);
    void UpdateTask(TaskDTO taskDTO);
    void UpdateTask(string projectName, string title, TaskDTO taskDTO, bool solve = false);
    void DeleteTask(TaskDTO taskDTO);
    void DeleteTask(string projectName, string title);
    CpmResultDTO GetCriticalPath(string projectName);
}