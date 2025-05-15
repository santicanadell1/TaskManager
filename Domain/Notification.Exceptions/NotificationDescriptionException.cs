namespace Domain.Exceptions.NotificationExceptions;

public class NotificationDescriptionException : NotificationException
{
    public NotificationDescriptionException()
        : base("Description cannot be null or empty.")
    {
    }
}