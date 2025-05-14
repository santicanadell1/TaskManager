namespace Service.Exceptions.UserServiceExceptions;

public class InvalidLoginCredentialsException : LoginException
{
    public InvalidLoginCredentialsException() 
        : base("User or password is incorrect, try again.") { }
}