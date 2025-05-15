namespace Domain.Exceptions.TaskExceptions;

public class TaskDurationException : TaskException
{
    public TaskDurationException()
        : base("Duration cannot be zero or negative.")
    {
    }
}