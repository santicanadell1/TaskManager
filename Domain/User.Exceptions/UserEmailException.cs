namespace Domain.Exceptions.UserExceptions;

public class UserEmailException : UserException
{
    public UserEmailException()
        : base("The user's email cannot be empty or have an invalid format.")
    {
    }
}