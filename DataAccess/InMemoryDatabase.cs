using Domain;
namespace DataAccess;

public class InMemoryDatabase
{
    public UserRepository users;
    public NotificationRepository notifications;

    public InMemoryDatabase()
    {
        users = new UserRepository();
        notifications = new NotificationRepository();
    }
}