namespace Domain.Exceptions.UserExceptions;

public class UserRoleAlreadyExistsException : UserException
{
    public UserRoleAlreadyExistsException(string role)
        : base($"Role '{role}' already exists for this user.")
    {
    }
}