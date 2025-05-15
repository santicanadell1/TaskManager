namespace Service.Exceptions.LoginExceptions;

public class UserNotFoundExceptionLogin : LoginException
{
    public UserNotFoundExceptionLogin()
        : base("User not found.")
    {
    }
}