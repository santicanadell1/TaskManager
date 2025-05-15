namespace DataAccess.Exceptions.UserRepositoryExceptions;

public class UserEmailIsDuplicatedException : Exception
{
    public UserEmailIsDuplicatedException() : base("The user email already exists.")
    {
    }
}