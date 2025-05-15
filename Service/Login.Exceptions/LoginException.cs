namespace Service.Exceptions.LoginExceptions;

public class LoginException : Exception
{
    public LoginException(string message)
        : base(message)
    {
    }
}