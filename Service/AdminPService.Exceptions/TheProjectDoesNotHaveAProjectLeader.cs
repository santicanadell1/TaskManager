namespace Service.Exceptions.LeaderPServiceException;

public class TheProjectDoesNotHaveAProjectLeader : Exception
{
    public TheProjectDoesNotHaveAProjectLeader() :
        base("the project does not have a project leader")
    {
    }
}