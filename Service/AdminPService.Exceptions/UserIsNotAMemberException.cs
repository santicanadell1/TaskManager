namespace Service.Exceptions.AdminPServiceExceptions;

public class UserIsNotAMemberException : Exception
{
    public UserIsNotAMemberException() : base("The user is not a member of the group.")
    {
    }
}