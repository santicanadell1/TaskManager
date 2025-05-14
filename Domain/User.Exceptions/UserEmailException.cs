namespace Domain.Exceptions.NotificationExceptions
{
    public class UserEmailException : UserException
    {
        public UserEmailException() 
            : base("The user's email cannot be empty or have an invalid format.") { }
    }
}