namespace Domain.Exceptions
{
    public class UserPasswordException : UserException
    {
        public UserPasswordException() 
            : base("The user's password cannot be empty.") { }
    }
}