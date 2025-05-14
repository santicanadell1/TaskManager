namespace DataAccess.Exceptions.ResourceRepositoryExceptions;

public class ResourceRepositoryExceptions : Exception
{
    public ResourceRepositoryExceptions(string message)
        : base(message)
    {
    }

    public override string ToString()
    {
        return $"ResourceRepositoryExceptions: {this.GetType().Name} - {this.Message}";
    }
}