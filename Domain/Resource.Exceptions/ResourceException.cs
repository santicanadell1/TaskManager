namespace Domain.Exceptions.ResourceExceptions;

public class ResourceException : Exception
{
    public ResourceException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"ResourceException: {GetType().Name} - {Message}";
    }
}