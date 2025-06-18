using DataAccess;
using Domain;
using Task = Domain.Task;

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

    public IRepository<User> UserRepository =>
        _userRepository ??= new UserRepository(_context);

    public IProjectRepository ProjectRepository =>
        _projectRepository ??= new ProjectRepository(_context);

    public IRepository<Notification> NotificationRepository =>
        _notificationRepository ??= new NotificationRepository(_context);

    public IRepository<Task> TaskRepository =>
        _taskRepository ??= new TaskRepository(_context);

    public IRepository<Resource> ResourceRepository =>
        _resourceRepository ??= new ResourceRepository(_context);
}