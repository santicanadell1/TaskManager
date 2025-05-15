namespace DataAccess.Exceptions.NotificationRepositoryExceptions;

public class NotificationRepositoryException : Exception
{
    public NotificationRepositoryException(string message)
        : base(message)
    {
    }

    public override string ToString()
    {
        return $"NotificationRepositoryException: {GetType().Name} - {Message}";
    }
}