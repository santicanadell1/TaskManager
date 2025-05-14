namespace Domain.Exceptions.NotificationExceptions
{
    public class TaskDescriptionException : TaskException
    {
        public TaskDescriptionException() 
            : base("Description cannot be empty or whitespace.") { }
    }
}