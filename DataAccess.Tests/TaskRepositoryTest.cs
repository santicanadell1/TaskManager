namespace DataAccess.Test;
using Domain;
[TestClass]
public class TaskRepositoryTest
{
    private TaskRepository _taskRepository;
    private AppDbContext _context;
    private InMemoryAppContextFactory _contextFactory;
    private Task _task;
    private Task _task2;

    [TestInitialize]
    public void Initialize()
    {
        _contextFactory = new InMemoryAppContextFactory();
        _context = _contextFactory.CreateDbContext();
        _taskRepository = new TaskRepository(_context);
        _task = new Task("Task1", "Description1", DateTime.Today, 2, new List<Task>(), new List<Task>(),new List<Resource>());
        _task2 = new Task("Task2","Description2",DateTime.Today, 2, new List<Task>(),new List<Task>(),new List<Resource>());
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
        
        var all = _context.Tasks.ToList();
        Assert.Equals(1, all.Count);
        Assert.Equals("Task1", all[0].Title);
        Assert.Equals("Description1", all[0].Description);
        Assert.Equals(2, all[0].Duration);
    }
}