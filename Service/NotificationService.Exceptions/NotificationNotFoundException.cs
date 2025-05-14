namespace Service.Exceptions.NotificationServiceExceptions;

public class NotificationNotFoundException : NotificationServiceException
{
    public NotificationNotFoundException() : base("Notification not found")
    {
    }
}