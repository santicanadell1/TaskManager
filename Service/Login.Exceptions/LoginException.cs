namespace Service.Exceptions.UserServiceExceptions;

public class LoginException : Exception
{
    public LoginException(string message) 
        : base(message) { }
}