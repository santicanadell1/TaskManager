namespace Service.Exceptions.CPMServiceExceptions;

public class CpmServiceException : Exception
{
    public CpmServiceException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"CpmServiceException: {GetType().Name} - {Message}";
    }
}