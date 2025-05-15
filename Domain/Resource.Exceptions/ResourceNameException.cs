namespace Domain.Exceptions.ResourceExceptions;

public class ResourceNameException : ResourceException
{
    public ResourceNameException()
        : base("The resource name cannot be empty.")
    {
    }
}