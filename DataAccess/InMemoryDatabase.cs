using Domain;
namespace DataAccess;

public class InMemoryDatabase
{
    public UserRepository users;
    public NotificationRepository notifications;
    public ProjectRepository projects;
    public ResourceRepository resources;

    public InMemoryDatabase()
    {
        users = new UserRepository();
        notifications = new NotificationRepository();
        projects = new ProjectRepository();
        resources = new ResourceRepository();
    }
}