namespace DataAccess.Exceptions.NotificationRepositoryExceptions;

public class NotificationNotFoundException : NotificationRepositoryException
{
    public NotificationNotFoundException()
        : base("The notification was not found in the repository.")
    {
    }
}