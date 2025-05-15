namespace Domain.Exceptions.UserExceptions;

public class UserBirthdayException : UserException
{
    public UserBirthdayException()
        : base("The user must be older than 18 years old and younger than 100 years old.")
    {
    }
}