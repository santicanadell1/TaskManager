namespace DataAccess.Exceptions.TaskRepositoryExceptions;

public class TaskAlreadyExistsException : TaskRepositoryExceptions
{
    public TaskAlreadyExistsException(string taskTitle)
        : base($"A task with the title '{taskTitle}' already exists in the repository.")
    {
    }
}