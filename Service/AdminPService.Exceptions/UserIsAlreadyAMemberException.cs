namespace Domain.Exceptions.NotificationExceptions;

public class UserIsAlreadyAMemberException:Exception
{
    public UserIsAlreadyAMemberException() : base("The user is already a member of the group."){}  
}