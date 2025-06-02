using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Task = Domain.Task;

namespace DataAccess;

public class TaskRepository : IRepository<Task>
{
    protected readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
    {
        _db = db;
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
    public void Update(int id, Task updatedTask)
    {
        if (updatedTask == null)
            throw new TaskNotFoundException();

        var existingTask = _db.Tasks.Find(id);
        if (existingTask == null)
            throw new TaskNotFoundException();
        
        existingTask.Title = updatedTask.Title;
        existingTask.Description = updatedTask.Description;
        existingTask.Duration = updatedTask.Duration;
        existingTask.ExpectedStartDate = updatedTask.ExpectedStartDate;
        existingTask.StartDate = updatedTask.StartDate;
        existingTask.EndDate = updatedTask.EndDate;
        existingTask.LatestStart = updatedTask.LatestStart;
        existingTask.LatestFinish = updatedTask.LatestFinish;
        existingTask.Slack = updatedTask.Slack;
        existingTask.IsCritical = updatedTask.IsCritical;
        existingTask.State = updatedTask.State;
        existingTask.PreviousTasks = updatedTask.PreviousTasks ?? new List<Task>();
        existingTask.SameTimeTasks = updatedTask.SameTimeTasks ?? new List<Task>();
        existingTask.Resources = updatedTask.Resources ?? new List<Resource>();

        try
        {
            _db.SaveChanges();
        }
        catch (Exception e)
        {
            throw new TaskRepositoryExceptions("The task can't be updated");
        }
    }
    public void Delete(Task task)
    {
        try
        {
            var existingTask = _db.Tasks.Find(task);
            if (existingTask == null)
                throw new TaskNotFoundException();

            _db.Set<Task>().Remove(existingTask);
            _db.SaveChanges();
        }
        catch (Exception)
        {
            throw new TaskRepositoryExceptions("The task can't be deleted");
        }
    }
}