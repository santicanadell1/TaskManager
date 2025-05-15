namespace Service.Exceptions.LoginExceptions;

public class InvalidLoginCredentialsException : LoginException
{
    public InvalidLoginCredentialsException()
        : base("User or password is incorrect, try again.")
    {
    }
}