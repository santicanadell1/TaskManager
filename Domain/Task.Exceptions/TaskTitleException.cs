namespace Domain.Exceptions.TaskExceptions;

public class TaskTitleException : TaskException
{
    public TaskTitleException()
        : base("Title cannot be empty or whitespace.")
    {
    }
}