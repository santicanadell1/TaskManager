namespace Service.MemberServiceException;

public class UserHasNoProjectsException:Exception
{
    public UserHasNoProjectsException() : base("The user has no projects."){}
}