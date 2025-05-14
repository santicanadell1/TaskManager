namespace Domain.Exceptions.NotificationExceptions;

public class NotificationNotFoundException : NotificationServiceException
{
    public NotificationNotFoundException() : base("Notification not found")
    {
    }
}