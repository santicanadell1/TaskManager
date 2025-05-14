namespace Domain.Exceptions.NotificationExceptions
{
    public class TaskResourceException : TaskException
    {
        public TaskResourceException(string message) 
            : base(message) { }
    }
}