namespace Domain.Exceptions.UserRepositoryExceptions;

public class UserNameIsDuplicatedException : Exception
{
    public UserNameIsDuplicatedException() : base("The user name already exists."){}
}