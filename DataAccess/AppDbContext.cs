using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class AppDbContext : DbContext
{
    public NotificationRepository notifications;
    public ProjectRepository projects;
    public ResourceRepository resources;
    public UserRepository users;

    public AppDbContext()
    {
        users = new UserRepository();
        notifications = new NotificationRepository();
        projects = new ProjectRepository();
        resources = new ResourceRepository();
    }
}