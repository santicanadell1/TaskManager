namespace Domain.Exceptions.ResourceExceptions;

public class ResourceTypeException : ResourceException
{
    public ResourceTypeException()
        : base("The resource type cannot be empty.")
    {
    }
}