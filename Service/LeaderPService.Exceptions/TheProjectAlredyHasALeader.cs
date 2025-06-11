namespace Service.Exceptions.LeaderPServiceException;

public class TheProjectAlredyHasALeader : LeaderPServiceException
{
    public TheProjectAlredyHasALeader() :
        base("The project already has a leader, a project can only have one leade")
    {
    }
}