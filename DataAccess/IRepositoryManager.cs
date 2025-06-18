using Domain;
using Task = Domain.Task;

namespace DataAccess;

public interface IRepositoryManager
{
    IRepository<User> UserRepository { get; }
    IProjectRepository ProjectRepository { get; }
    IRepository<Notification> NotificationRepository { get; }
    IRepository<Task> TaskRepository { get; }
    IRepository<Resource> ResourceRepository { get; }
}