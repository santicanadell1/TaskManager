namespace Domain.Exceptions;

public class InvalidOldPasswordException : AdminSServiceException
{
    public InvalidOldPasswordException() : 
        base("The old password is invalid."){}

}