namespace Service.Exceptions.AdminPServiceExceptions;

public class UserIsNotAdminException : Exception
{
    public UserIsNotAdminException() : base("The user is not a admin project.")
    {
    }
}