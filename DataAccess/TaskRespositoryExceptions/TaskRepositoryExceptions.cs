namespace DataAccess.Exceptions.TaskRepositoryExceptions;

public class TaskRepositoryExceptions : Exception
{
    public TaskRepositoryExceptions(string message)
        : base(message)
    {
    }

    public override string ToString()
    {
        return $"TaskRepositoryExceptions: {GetType().Name} - {Message}";
    }
}