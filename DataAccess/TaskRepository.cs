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
}