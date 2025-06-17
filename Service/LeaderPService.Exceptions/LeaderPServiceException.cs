namespace Service.Exceptions.LeaderPServiceException;

public class LeaderPServiceException : Exception
{
    public LeaderPServiceException(string message) : base(message)
    {
    }

    public override string ToString()
    {
        return $"LeaderPServiceExeption: {GetType().Name} - {Message}";
    }
}