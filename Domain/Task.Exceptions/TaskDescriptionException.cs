namespace Domain.Exceptions
{
    public class TaskDescriptionException : TaskException
    {
        public TaskDescriptionException() 
            : base("Description cannot be empty or whitespace.") { }
    }
}