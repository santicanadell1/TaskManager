using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Task = Domain.Task;

namespace DataAccess;

public class TaskRepository
{
    private static int _nextId;
    protected readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
    {
        _db = db;
        _nextId = 1;
    }
    public void Add(Task task)
    {
        if (task == null)
            throw new TaskNotFoundException();

        ValidateDuplicateTitle(task.Title);

        try
        {
            _db.Set<Task>().Add(task);
            _db.SaveChanges();
        }
        catch (Exception)
        {
            throw new TaskRepositoryExceptions("The task can't be added");
        }
    }
    private void ValidateDuplicateTitle(string title)
    {
        if (_db.Set<Task>().Any(t => t.Title == title))
        {
            throw new TaskAlreadyExistsException(title);
        }
    }
    public List<Task> GetAll()
    {
        return _db.Set<Task>().ToList();
    }
    public Task? Get(Func<Task, bool> filter)
    {
        return _db.Set<Task>().FirstOrDefault(filter);
    }
    
}