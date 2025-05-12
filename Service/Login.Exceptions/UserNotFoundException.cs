namespace Service.Exceptions;

public partial class UserNotFoundExceptionLogin : LoginException
{
    public UserNotFoundExceptionLogin() 
        : base("User not found.") { }
}