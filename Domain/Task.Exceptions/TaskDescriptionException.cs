namespace Domain.Exceptions.TaskExceptions;

public class TaskDescriptionException : TaskException
{
    public TaskDescriptionException()
        : base("Description cannot be empty or whitespace.")
    {
    }
}