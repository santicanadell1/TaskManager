namespace Domain.Exceptions
{
    public class UserNotFoundException : UserServiceException
    {
        public UserNotFoundException() : base("User not found or does not exist.")
        {
        }
    }
}