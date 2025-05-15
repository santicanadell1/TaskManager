namespace Domain.Exceptions.TaskExceptions;

public class TaskResourceException : TaskException
{
    public TaskResourceException(string message)
        : base(message)
    {
    }
}