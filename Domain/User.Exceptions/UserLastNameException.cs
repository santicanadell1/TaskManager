namespace Domain.Exceptions.UserExceptions;

public class UserLastNameException : UserException
{
    public UserLastNameException()
        : base("The user's last name cannot be empty or just spaces.")
    {
    }
}