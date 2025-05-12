namespace Service.Exceptions
{
    public class InvalidUserEmailException : UserServiceException
    {
        public InvalidUserEmailException() : base("Invalid email address or this email has already registered.")
        {
        }
    }
}