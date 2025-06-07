namespace DataAccess;

public interface IRepositoryManager
{
    UserRepository UserRepository { get; }
    ProjectRepository ProjectRepository { get; }
    NotificationRepository NotificationRepository { get; }
    TaskRepository TaskRepository { get; }
    ResourceRepository ResourceRepository { get; }
}