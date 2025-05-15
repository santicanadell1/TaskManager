namespace Domain.Exceptions.TaskExceptions;

public class TaskPreviousTaskException : TaskException
{
    public TaskPreviousTaskException(string message)
        : base(message)
    {
    }
}