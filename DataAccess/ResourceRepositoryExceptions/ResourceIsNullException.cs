namespace DataAccess.ResourceRepositoryExceptions;

public class ResourceIsNullException : ResourceRepositoryExceptions
{
    public ResourceIsNullException() : base("The resource was null.")
    {}
}