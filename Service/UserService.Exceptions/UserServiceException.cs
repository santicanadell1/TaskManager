namespace Service.Exceptions.UserServiceExceptions;

public class UserServiceException : Exception
{
    public UserServiceException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"UserException: {GetType().Name} - {Message}";
    }
}