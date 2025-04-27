using DataAccess;
using Domain;
using Service;

[TestClass]
public class LoginTests
{
    private Login _login;
    private UserRepository _userRepository;

    [TestInitialize]
    public void Setup()
    {
        _userRepository = new UserRepository(); 
        _login = new Login(_userRepository); 
    }

    [TestMethod]
    public void Login_ShouldLoginSuccessfully_WithValidCredentials()
    {
        // Arrange
        var email = "john.doe@example.com";
        var password = "Password123@";
        var roles = new List<Rol> { Rol.AdminSystem, Rol.AdminProject};

        // Add a user to the repository for testing
        _userRepository.AddUser(new User
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password, // Correct password
            Roles = roles
        });

        // Act
        _login.LoginUser(email, password);

        // Assert
        var loggedUser = _login.GetLoggedUser();
        Assert.IsNotNull(loggedUser);
        Assert.AreEqual("John", loggedUser.FirstName);
        Assert.AreEqual("Doe", loggedUser.LastName);
        Assert.AreEqual("john.doe@example.com", loggedUser.Email);
    }
    
    [TestMethod]
    public void Logout_ShouldLogoutSuccessfully_WhenUserIsLoggedIn()
    {
        // Arrange
        var email = "john.doe@example.com";
        var password = "Password123@";
        var roles = new List<Rol> { Rol.AdminSystem, Rol.AdminProject };

        // Crear un usuario de prueba
        var user = new User
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Roles = roles
        };

        _userRepository.AddUser(user);

        // Act 
        _login.LoginUser(email, password);

        // Assert 
        var loggedUserBeforeLogout = _login.GetLoggedUser();
        Assert.IsNotNull(loggedUserBeforeLogout);

        // Act - Hacer logout
        _login.Logout();

        // Assert 
        var loggedUserAfterLogout = _login.GetLoggedUser();
        Assert.IsNull(loggedUserAfterLogout);
    }
}
}