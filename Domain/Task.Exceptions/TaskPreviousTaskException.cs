namespace Domain.Exceptions.NotificationExceptions
{
    public class TaskPreviousTaskException : TaskException
    {
        public TaskPreviousTaskException(string message)
            : base(message)
        {
        }
    }
}