namespace Service.Exceptions.UserServiceExceptions
{
    public class UserServiceNotFoundException : UserServiceException
    {
        public UserServiceNotFoundException() : base("User not found or does not exist.")
        {
        }
    }
}