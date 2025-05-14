namespace Service.Exceptions.UserServiceExceptions;

public partial class UserNotFoundExceptionLogin : LoginException
{
    public UserNotFoundExceptionLogin() 
        : base("User not found.") { }
}