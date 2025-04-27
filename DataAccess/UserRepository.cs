using Domain;

namespace DataAccess;

public class UserRepository
{
    private readonly DataAccess _dataAccess;

    public UserRepository(DataAccess dataAccess)
    {
        _dataAccess = dataAccess;   
    }

    public void Add(User user)
    {
        if (user == null)
        {
            throw new ArgumentException("User cannot be null.");
        }

        try
        {
            _dataAccess.Users.Add(user);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException("Error while adding the user.", ex);
        }
    }
    
    
    
}