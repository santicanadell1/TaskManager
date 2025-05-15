namespace Domain.Exceptions.TaskExceptions;

public class TaskException : Exception
{
    public TaskException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"TaskException: {GetType().Name} - {Message}";
    }
}