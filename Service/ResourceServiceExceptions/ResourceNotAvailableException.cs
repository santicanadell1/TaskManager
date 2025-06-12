namespace Service.Exceptions.ResourceServiceExceptions;

public class ResourceNotAvailableException : ResourceServiceException
{
    public ResourceNotAvailableException() : base("The resource is not available.")
    {
    }
}