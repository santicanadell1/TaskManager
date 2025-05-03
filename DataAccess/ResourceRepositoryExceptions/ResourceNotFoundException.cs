namespace DataAccess.ResourceRepositoryExceptions;

public class ResourceNotFoundException : ResourceRepositoryExceptions
{
    public ResourceNotFoundException()
        : base("Resource not found"){}
}