using DataAccess;
using Service;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class TaskController
{
    private readonly ITaskService _taskService;

    public TaskController(IRepositoryManager repositoryManager)
    {
        var cpmService = new CpmService();
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

    public void AddTaskToProject(string projectName, TaskDTO newCreatedTask, bool solved = false)
    {
        _taskService.AddTask(projectName, newCreatedTask, solved);
    }

    public void UpdateTask(string projectName, string taskToUpdate, TaskDTO newUpdatedTask, bool solved = false)
    {
        _taskService.UpdateTask(projectName, taskToUpdate, newUpdatedTask, solved);
    }

    public void DeleteTask(string projectName, string taskToDelete)
    {
        _taskService.DeleteTask(projectName, taskToDelete);
    }
}