namespace Service.Exceptions.LoginExceptions;

public partial class UserNotFoundExceptionLogin : LoginException
{
    public UserNotFoundExceptionLogin() 
        : base("User not found.") { }
}