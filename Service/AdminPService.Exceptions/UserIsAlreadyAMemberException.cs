namespace Service.Exceptions.AdminPServiceExceptions;

public class UserIsAlreadyAMemberException : Exception
{
    public UserIsAlreadyAMemberException() : base("The user is already a member of the group.")
    {
    }
}