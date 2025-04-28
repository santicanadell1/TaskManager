namespace Domain.Exceptions
{
    public class UserBirthdayException : UserException
    {
        public UserBirthdayException() 
            : base("The user's birthday cannot be after today's date.") { }
    }
}