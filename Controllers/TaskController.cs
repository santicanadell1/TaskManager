using DataAccess;
using Service;
using Service.Models;

namespace Controllers;

public class TaskController
{
    private readonly TaskService _taskService;

    public TaskController(IRepositoryManager repositoryManager)
    {
        CpmService cpmService = new CpmService();
        _taskService = new TaskService(repositoryManager, cpmService);
    }

    public TaskDTO GetTask(string projectName, string titleOfTaskToBeRecibed)
    {
        return _taskService.GetTask(projectName, titleOfTaskToBeRecibed);
    }

    public List<TaskDTO> GetAllTasksForAProject(string projectName)
    {
        return _taskService.GetTasks(projectName);
    }

    public void AddTaskToProject(string projectName, TaskDTO newCreatedTask)
    {
        _taskService.AddTask(projectName, newCreatedTask);
    }

    public void UpdateTask(string projectName, string taskToUpdate, TaskDTO newUpdatedTask)
    {
        _taskService.UpdateTask(projectName, taskToUpdate, newUpdatedTask);
    }

    public void DeleteTask(string projectName, string taskToDelete)
    {
        _taskService.DeleteTask(projectName, taskToDelete);
    }
}