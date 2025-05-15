namespace Domain.Exceptions.UserExceptions;

public class UserTaskException : Exception
{
    public UserTaskException(string message) : base(message)
    {
    }
}