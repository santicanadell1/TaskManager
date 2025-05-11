using DataAccess;
using Domain;
using Domain.Exceptions;
using Service.Models;
using Task = Domain.Task;

namespace Service.Test;

[TestClass]
public class TaskServiceTest
{
    private InMemoryDatabase _database;
    private TaskService _taskService;
    private Project _genericProject;
    private Task _task1;
    private Task _task2;
    private TaskDTO _taskDTO1;
    private TaskDTO _taskDTO2;

    [TestInitialize]
    public void Setup()
    {
        _database = new InMemoryDatabase();
        _taskService = new TaskService(_database);

        _genericProject = new Project("Generic Project", "Description", DateTime.Now);
        _database.projects.AddProject(_genericProject);

        _taskDTO1 = new TaskDTO
        {
            Title = "Task 1",
            Description = "Description of Task 1",
            ExpectedStartDate = DateTime.Now,
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO>()
        };

        _taskDTO2 = new TaskDTO
        {
            Title = "Task 2",
            Description = "Description of Task 2",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 3,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO>()
        };

        _task1 = new Task(
            _taskDTO1.Title,
            _taskDTO1.Description,
            _taskDTO1.ExpectedStartDate,
            _taskDTO1.Duration,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );
        _task2 = new Task(
            _taskDTO2.Title,
            _taskDTO2.Description,
            _taskDTO2.ExpectedStartDate,
            _taskDTO2.Duration,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );

        _database.projects.AddTask("Generic Project", _task1);
        _database.projects.AddTask("Generic Project", _task2);
    }

    [TestMethod]
    public void CalculateEarlyStart_ShouldReturnCorrectDate_WhenNoPreviousTasks()
    {
        DateTime earlyStart = _taskService.CalculateEarlyStart(_task1);
        Assert.AreEqual(_task1.ExpectedStartDate, earlyStart);
    }

    [TestMethod]
    public void CalculateEarlyFinish_ShouldReturnCorrectDate_WhenGivenDuration()
    {
        DateTime earlyFinish = _taskService.CalculateEarlyFinish(_task1);
        Assert.AreEqual(_task1.ExpectedStartDate.AddDays(_task1.Duration), earlyFinish);
    }

    [TestMethod]
    public void CalculateLateFinish_ShouldReturnEarlyFinish_WhenNoPreviousTasks()
    {
        DateTime lateFinish = _taskService.CalculateLateFinish(_task1);
        Assert.AreEqual(_task1.ExpectedStartDate.AddDays(_task1.Duration), lateFinish);
    }

    [TestMethod]
    public void CalculateLateStart_ShouldReturnExpectedStartDate_WhenNoPreviousTasks()
    {
        DateTime lateStart = _taskService.CalculateLateStart(_task1);
        Assert.AreEqual(_task1.ExpectedStartDate, lateStart);
    }

    [TestMethod]
    public void CalculateLateStart_ShouldReturnCorrectDate_WhenPreviousTasksExist()
    {
        _task2.EndDate = new DateTime(2025, 4, 30);
        _taskDTO1.PreviousTasks.Add(_taskDTO2);
        _task1.PreviousTasks.Add(_task2);

        DateTime lateStart = _taskService.CalculateLateStart(_task1);
        Assert.AreEqual(_task2.EndDate, lateStart);
    }

    [TestMethod]
    public void IsCritical_ShouldReturnFalse_WhenTaskIsNotCritical()
    {
        _task2.EndDate = new DateTime(2025, 4, 30);
        _task1.EndDate = new DateTime(2025, 5, 7);
        _taskDTO1.PreviousTasks.Add(_taskDTO2);
        _task1.PreviousTasks.Add(_task2);

        bool isCritical = _taskService.IsCritical(_task1);
        Assert.IsFalse(isCritical);
    }

    [TestMethod]
    public void AddTask_ShouldAddTask_WhenTaskIsValid()
    {
        var taskDTO = new TaskDTO
        {
            Title = "Test Task",
            Description = "Test Description",
            ExpectedStartDate = DateTime.Now.AddDays(1),
            Duration = 5,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO>()
        };

        _taskService.AddTask("Generic Project", taskDTO);

        var project = _database.projects.GetProject(p => p.Name == "Generic Project");
        var tasks = project.Tasks;
        Assert.AreEqual(3, tasks.Count);
        Assert.AreEqual("Test Task", tasks[2].Title);
    }


    [TestMethod]
    public void DeleteTask_ShouldDeleteTask_WhenTaskExists()
    {
        _taskService.DeleteTask("Generic Project", _task1.Id);

        var project = _database.projects.GetProject(p => p.Name == "Generic Project");
        Assert.AreEqual(1, project.Tasks.Count);
        Assert.AreEqual("Task 2", project.Tasks[0].Title);
    }

    [TestMethod]
    public void UpdateTask_ShouldUpdateTask_WhenTaskExists()
    {
        var updateDTO = new TaskDTO
        {
            Title = "Updated Task 1",
            Description = "Updated Description",
            ExpectedStartDate = DateTime.Now.AddDays(1),
            Duration = 6,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        _taskService.UpdateTask("Generic Project", _task1.Id, updateDTO);

        var project = _database.projects.GetProject(p => p.Name == "Generic Project");
        var updatedTask = project.Tasks.FirstOrDefault(t => t.Id == _task1.Id);

        Assert.AreEqual("Updated Task 1", updatedTask.Title);
        Assert.AreEqual("Updated Description", updatedTask.Description);
        Assert.AreEqual(6, updatedTask.Duration);
    }


    [TestMethod]
    public void GetTasks_ShouldReturnAllTasks_WhenProjectExists()
    {
        var tasks = _taskService.GetTasks("Generic Project");

        Assert.AreEqual(2, tasks.Count);
        Assert.AreEqual("Task 1", tasks[0].Title);
        Assert.AreEqual("Task 2", tasks[1].Title);
    }

    [TestMethod]
    public void GetTask_ShouldReturnTask_WhenTaskExists()
    {
        var taskDTO = _taskService.GetTask("Generic Project", _task1.Id);

        Assert.AreEqual("Task 1", taskDTO.Title);
        Assert.AreEqual("Description of Task 1", taskDTO.Description);
        Assert.AreEqual(5, taskDTO.Duration);
    }
}