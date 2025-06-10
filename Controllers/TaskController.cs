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
}