namespace Service.Exceptions.AdminSServiceExceptions;

public class InvalidOldPasswordException : AdminSServiceException
{
    public InvalidOldPasswordException() :
        base("The old password is invalid.")
    {
    }
}