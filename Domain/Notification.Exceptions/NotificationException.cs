namespace Domain.Exceptions.NotificationExceptions;

public class NotificationException : Exception
{
    public NotificationException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"NotificationException: {GetType().Name} - {Message}";
    }
}