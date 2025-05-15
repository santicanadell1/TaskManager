namespace DataAccess.Exceptions.UserRepositoryExceptions;

public class UserNotFoundException : UserRepositoryExceptions
{
    public UserNotFoundException()
        : base("The user was not found in the repository.")
    {
    }
}