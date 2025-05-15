namespace Service.Exceptions.ResourceServiceExceptions;

public class NoResourcesFoundException : ResourceServiceException
{
    public NoResourcesFoundException() : base("No resources found")
    {
    }
}