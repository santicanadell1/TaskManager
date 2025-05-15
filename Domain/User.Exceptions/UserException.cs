namespace Domain.Exceptions.UserExceptions;

public class UserException : Exception
{
    public UserException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"UserException: {GetType().Name} - {Message}";
    }
}