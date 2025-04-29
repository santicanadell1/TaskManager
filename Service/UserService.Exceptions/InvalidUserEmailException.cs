namespace Domain.Exceptions
{
    public class InvalidUserEmailException : UserServiceException
    {
        public InvalidUserEmailException() : base("Invalid email address")
        {
        }
    }
}