namespace DataAccess.Exceptions.UserRepositoryExceptions;

public class UserRepositoryExceptions : Exception
{
    public UserRepositoryExceptions(string message)
        : base(message)
    {
    }

    public override string ToString()
    {
        return $"UserRepositoryExceptions: {GetType().Name} - {Message}";
    }
}