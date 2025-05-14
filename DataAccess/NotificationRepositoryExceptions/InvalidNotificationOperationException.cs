namespace DataAccess.Exceptions.NotificationRepositoryExceptions
{
    public class InvalidNotificationOperationException : NotificationRepositoryException
    {
        public InvalidNotificationOperationException() 
            : base("Invalid operation performed on the notification.") { }
    }
}