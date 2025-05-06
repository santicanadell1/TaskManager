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

    [TestInitialize]
    public void Setup()
    {
        _database = new InMemoryDatabase();
        _taskService = new TaskService(_database);

        _genericProject = new Project("Generic Project", "Description", DateTime.Now);
        _database.projects.AddProject(_genericProject);
    }


    [TestMethod]
    public void CalculateEarlyStart_ShouldReturnCorrectDate_WhenNoPreviousTasks()
    {
        var task = new Task(
            "Task 1",
            "Description of Task 1",
            new DateTime(2025, 5, 1),
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );

        DateTime earlyStart = _taskService.CalculateEarlyStart(task);

        Assert.AreEqual(new DateTime(2025, 5, 1), earlyStart);
    }

    [TestMethod]
    public void CalculateEarlyFinish_ShouldReturnCorrectDate_WhenGivenDuration()
    {
        var task = new Task(
            "Task 1",
            "Description of Task 1",
            new DateTime(2025, 5, 1),
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );

        DateTime earlyFinish = _taskService.CalculateEarlyFinish(task);

        Assert.AreEqual(new DateTime(2025, 5, 6), earlyFinish);
    }

    [TestMethod]
    public void CalculateLateFinish_ShouldReturnEarlyFinish_WhenNoPreviousTasks()
    {
        var task = new Task(
            "Task 1",
            "Description of Task 1",
            new DateTime(2025, 5, 1),
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );

        DateTime lateFinish = _taskService.CalculateLateFinish(task);

        Assert.AreEqual(new DateTime(2025, 5, 6), lateFinish);
    }

    [TestMethod]
    public void CalculateLateStart_ShouldReturnExpectedStartDate_WhenNoPreviousTasks()
    {
        var task = new Task(
            "Task 1",
            "Description of Task 1",
            new DateTime(2025, 5, 1),
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        );

        DateTime lateStart = _taskService.CalculateLateStart(task);

        Assert.AreEqual(new DateTime(2025, 5, 1), lateStart);
    }

    [TestMethod]
    public void CalculateLateStart_ShouldReturnCorrectDate_WhenPreviousTasksExist()
    {
        var previousTask = new Task(
            "Previous Task",
            "Previous task description",
            new DateTime(2025, 4, 25),
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        )
        {
            EndDate = new DateTime(2025, 4, 30)
        };

        var task = new Task(
            "Task 1",
            "Description of Task 1",
            new DateTime(2025, 5, 1),
            5,
            new List<Task> { previousTask },
            new List<Task>(),
            new List<Resource>()
        );

        DateTime lateStart = _taskService.CalculateLateStart(task);

        Assert.AreEqual(new DateTime(2025, 4, 30), lateStart);
    }

    [TestMethod]
    public void IsCritical_ShouldReturnFalse_WhenTaskIsNotCritical()
    {
        var previousTask = new Task(
            "Previous Task",
            "Previous task description",
            new DateTime(2025, 4, 25),
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource>()
        )
        {
            EndDate = new DateTime(2025, 4, 30)
        };

        var task = new Task(
            "Task 1",
            "Description of Task 1",
            new DateTime(2025, 5, 1),
            5,
            new List<Task> { previousTask },
            new List<Task>(),
            new List<Resource>()
        )
        {
            EndDate = new DateTime(2025, 5, 7)
        };

        bool isCritical = _taskService.IsCritical(task);

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
            Duration = 5
        };

        _taskService.AddTask("Generic Project", taskDTO);

        var project = _database.projects.GetProject(p => p.Name == "Generic Project");
        var tasks = project.Tasks;
        Assert.AreEqual(1, tasks.Count);
        Assert.AreEqual("Test Task", tasks[0].Title);
    }

    [TestMethod]
    public void DeleteTask_ShouldDeleteTask_WhenTaskExists()
    {
        var task1 = new Task("Task 1", "Description", DateTime.Now, 5, new List<Task>(), new List<Task>(),
            new List<Resource>());
        var task2 = new Task("Task 2", "Description", DateTime.Now.AddDays(2), 3, new List<Task>(), new List<Task>(),
            new List<Resource>());

        _database.projects.AddTask("Generic Project", task1);
        _database.projects.AddTask("Generic Project", task2);

        _taskService.DeleteTask("Generic Project", task1.Id);

        var project = _database.projects.GetProject(p => p.Name == "Generic Project");
        Assert.AreEqual(1, project.Tasks.Count);
        Assert.AreEqual("Task 2", project.Tasks[0].Title);
    }
}