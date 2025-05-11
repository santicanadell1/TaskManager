namespace Service.Exceptions;

public class UserNotFoundException : LoginException
{
    public UserNotFoundException() 
        : base("User not found.") { }
}