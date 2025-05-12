namespace Service.Exceptions;

public partial class UserNotFoundException : LoginException
{
    public UserNotFoundException() 
        : base("User not found.") { }
}