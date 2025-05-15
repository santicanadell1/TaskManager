namespace Service.Exceptions.UserServiceExceptions;

public class NoUsersFoundException : UserServiceException
{
    public NoUsersFoundException() : base("The user list is empty or does not exist.")
    {
    }
}