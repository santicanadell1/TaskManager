namespace DataAccess.Exceptions.ResourceRepositoryExceptions;

public class ResourceRepositoryExceptions : Exception
{
    public ResourceRepositoryExceptions(string message)
        : base(message)
    {
    }

    public override string ToString()
    {
        return $"ResourceRepositoryExceptions: {GetType().Name} - {Message}";
    }
}