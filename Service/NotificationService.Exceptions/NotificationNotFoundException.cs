namespace Domain.Exceptions;

public class NotificationNotFoundException : NotificationServiceException
{
    public NotificationNotFoundException() : base("Notification not found")
    {
    }
}