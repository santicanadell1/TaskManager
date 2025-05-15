namespace Service.Exceptions.AdminSServiceExceptions;

public class AdminSServiceException : Exception
{
    public AdminSServiceException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"AdminSServiceException: {GetType().Name} - {Message}";
    }
}