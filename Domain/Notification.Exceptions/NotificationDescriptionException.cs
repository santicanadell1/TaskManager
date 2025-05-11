namespace Domain.Exceptions
{
    public class NotificationDescriptionException : NotificationException
    {
        public NotificationDescriptionException() 
            : base("Description cannot be null or empty.") { }
    }
}