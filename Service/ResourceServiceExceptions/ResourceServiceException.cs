namespace Service.Exceptions.ResourceServiceExceptions;

public class ResourceServiceException : Exception
{
    public ResourceServiceException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"ResourceServiceException: {GetType().Name} - {Message}";
    }
}