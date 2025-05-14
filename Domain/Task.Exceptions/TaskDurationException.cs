namespace Domain.Exceptions.NotificationExceptions
{
    public class TaskDurationException : TaskException
    {
        public TaskDurationException() 
            : base("Duration cannot be zero or negative.") { }
    }
}