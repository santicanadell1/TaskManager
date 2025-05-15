namespace DataAccess;

public class InMemoryDatabase
{
    public NotificationRepository notifications;
    public ProjectRepository projects;
    public ResourceRepository resources;
    public UserRepository users;

    public InMemoryDatabase()
    {
        users = new UserRepository();
        notifications = new NotificationRepository();
        projects = new ProjectRepository();
        resources = new ResourceRepository();
    }
}