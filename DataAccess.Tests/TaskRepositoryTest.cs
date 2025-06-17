using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Task = Domain.Task;

namespace DataAccess.Test;

[TestClass]
public class TaskRepositoryTest
{
    private AppDbContext _context;
    private InMemoryAppContextFactory _contextFactory;
    private Task _task;
    private Task _task2;
    private TaskRepository _taskRepository;

    [TestInitialize]
    public void Initialize()
    {
        _contextFactory = new InMemoryAppContextFactory();
        _context = _contextFactory.CreateDbContext();
        _taskRepository = new TaskRepository(_context);
        _task = new Task("Task1", "Description1", DateTime.Today, 2, new List<Task>(), new List<Task>(),
            new List<Resource>());
        _task2 = new Task("Task2", "Description2", DateTime.Today, 2, new List<Task>(), new List<Task>(),
            new List<Resource>());
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context.Database.EnsureDeleted();
    }

    [TestMethod]
    public void AddTask_AddsTask_WhenValidTaskIsPassed()
    {
        _taskRepository.Add(_task);
        Assert.AreEqual(1, _taskRepository.GetAll().Count);
        ;
    }

    [TestMethod]
    [ExpectedException(typeof(TaskAlreadyExistsException))]
    public void AddTask_ThrowsTaskTitleIsDuplicatedException_WhenTitleAlreadyExists()
    {
        _taskRepository.Add(_task);
        _taskRepository.Add(_task);
    }

    [TestMethod]
    public void Get_ReturnsMatchingTask_WhenFilterMatches()
    {
        _taskRepository.Add(_task);
        var found = _taskRepository.Get(t => t.Title == "Task1");
        Assert.IsNotNull(found);
        Assert.AreEqual("Task1", found!.Title);
        Assert.AreEqual("Description1", found.Description);
    }

    [TestMethod]
    public void Update_UpdatesAllFields_WhenTaskExists()
    {
        _taskRepository.Add(_task);

        var taskId = (int)_taskRepository.Get(t => t.Title == "Task1").Id;

        Assert.IsTrue(taskId > 0, "Task ID was not assigned correctly.");

        _task2.Id = taskId;

        _taskRepository.Update(_task2);

        var found = _taskRepository.Get(t => t.Id == taskId);

        Assert.IsNotNull(found);

        Assert.AreEqual("Task2", found.Title);
        Assert.AreEqual("Description2", found.Description);
    }


    [TestMethod]
    public void Update_UpdatesAllFields_Test2()
    {
        _taskRepository.Add(_task);
        _taskRepository.Add(_task2);

        var taskId = (int)_taskRepository.Get(t => t.Title == "Task2").Id;

        Assert.IsTrue(taskId > 0, "Task ID was not assigned correctly.");

        var _task3 = new Task("Task3", "Description3", DateTime.Today, 2, new List<Task>(), new List<Task>(),
            new List<Resource>());
        _task3.Id = taskId;

        _taskRepository.Update(_task3);

        var found = _taskRepository.Get(t => t.Id == taskId);

        Assert.IsNotNull(found);

        Assert.AreEqual("Task3", found.Title);
        Assert.AreEqual("Description3", found.Description);
    }


    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void Update_ThrowsTaskNotFoundException_WhenIdDoesNotExist()
    {
        _taskRepository.Update(_task);
    }

    [TestMethod]
    public void Delete_RemovesTask_WhenIdExists()
    {
        _taskRepository.Add(_task);
        Assert.AreEqual(1, _taskRepository.GetAll().Count);
        _taskRepository.Delete(_task);
        Assert.AreEqual(0, _taskRepository.GetAll().Count);
    }
}