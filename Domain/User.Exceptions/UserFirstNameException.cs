namespace Domain.Exceptions
{
    public class UserFirstNameException : UserException
    {
        public UserFirstNameException() 
            : base("The user's first name cannot be empty or just spaces.") { }
    }
}