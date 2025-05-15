namespace Domain.Exceptions.UserExceptions;

public class UserRoleNotFoundException : UserException
{
    public UserRoleNotFoundException(string role)
        : base($"Role '{role}' does not exist for this user.")
    {
    }
}