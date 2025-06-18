namespace Service.Exceptions.LeaderPServiceException;

public class UnauthorizedLeaderAccessException : LeaderPServiceException
{
    public UnauthorizedLeaderAccessException() :
        base("The user is not authorized to access this resource.")
    {
    }
}