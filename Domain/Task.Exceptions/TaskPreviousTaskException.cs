namespace Domain.Exceptions
{
    public class TaskPreviousTaskException : TaskException
    {
        public TaskPreviousTaskException(string message)
            : base(message)
        {
        }
    }
}