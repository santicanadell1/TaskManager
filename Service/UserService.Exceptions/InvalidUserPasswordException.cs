namespace Service.Exceptions.UserServiceExceptions;

public class InvalidUserPasswordException : UserServiceException
{
    public InvalidUserPasswordException() : base(
        "The password does not meet the minimum requirements. Please ensure it meets the following conditions:\n" +
        "- Minimum length: 8 characters.\n" +
        "- At least one uppercase letter (A-Z).\n" +
        "- At least one lowercase letter (a-z).\n" +
        "- At least one number (0-9).\n" +
        "- At least one special character (@, #, $, etc.).")
    {
    }
}