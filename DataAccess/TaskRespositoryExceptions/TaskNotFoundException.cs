namespace DataAccess.Exceptions.TaskRepositoryExceptions;

public class TaskNotFoundException : TaskRepositoryExceptions
{
    public TaskNotFoundException()
        : base("The task was not found in the repository.")
    {
    }
}