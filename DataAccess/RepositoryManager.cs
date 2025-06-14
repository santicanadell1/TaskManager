using DataAccess;

public class RepositoryManager : IRepositoryManager
{
    private readonly AppDbContext _context;
    private NotificationRepository _notificationRepository;
    private ProjectRepository _projectRepository;
    private ResourceRepository _resourceRepository;
    private TaskRepository _taskRepository;

    private UserRepository _userRepository;

    public RepositoryManager(AppDbContext context)
    {
        _context = context;
    }

    public UserRepository UserRepository =>
        _userRepository ??= new UserRepository(_context);

    public ProjectRepository ProjectRepository =>
        _projectRepository ??= new ProjectRepository(_context);

    public NotificationRepository NotificationRepository =>
        _notificationRepository ??= new NotificationRepository(_context);

    public TaskRepository TaskRepository =>
        _taskRepository ??= new TaskRepository(_context);

    public ResourceRepository ResourceRepository =>
        _resourceRepository ??= new ResourceRepository(_context);
}