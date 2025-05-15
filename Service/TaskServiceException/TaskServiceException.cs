namespace Service.Exceptions.TaskServiceExceptions;

public class TaskServiceException : Exception
{
    public TaskServiceException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"TaskServiceException: {GetType().Name} - {Message}";
    }
}