using DataAccess;

namespace Service;

public class Login
{
    private readonly UserRepository _userRepository;

    public Login(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public UserDTO GetLoggedUser()
    {
        return LoggedUser.Current;
    }
}