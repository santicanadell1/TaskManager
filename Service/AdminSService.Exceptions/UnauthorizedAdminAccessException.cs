namespace Service.Exceptions.AdminSServiceExceptions;

public class UnauthorizedAdminAccessException : AdminSServiceException
{
    public UnauthorizedAdminAccessException() :
        base("The user is not authorized to access this resource.")
    {
    }
}