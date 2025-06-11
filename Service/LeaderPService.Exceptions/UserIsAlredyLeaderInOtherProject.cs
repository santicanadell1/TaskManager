namespace Service.Exceptions.LeaderPServiceException;

public class UserIsAlredyLeaderInOtherProject : LeaderPServiceException
{
    public UserIsAlredyLeaderInOtherProject() :
        base("The user is leader in other project")
    {
    }
}