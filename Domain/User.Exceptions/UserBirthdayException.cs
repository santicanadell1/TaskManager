namespace Domain.Exceptions.UserExceptions
{
    public class UserBirthdayException : UserException
    {
        public UserBirthdayException() 
            : base("The user must be older than 18 years old.") { }
    }
}