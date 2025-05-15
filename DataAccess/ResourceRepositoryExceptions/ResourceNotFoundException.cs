namespace DataAccess.Exceptions.ResourceRepositoryExceptions;

public class ResourceNotFoundException : ResourceRepositoryExceptions
{
    public ResourceNotFoundException()
        : base("Resource not found")
    {
    }
}