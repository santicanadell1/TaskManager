using DataAccess;
using Domain;
using Interface.Components.Pages.LoginPages;

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
        _userRepository.Add(new User
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password, // Correct password
            Roles = roles
        });

        // Act
        _login.Login(email, password);

        // Assert
        var loggedUser = _login.GetLoggedUser();
        Assert.IsNotNull(loggedUser);
        Assert.AreEqual("John", loggedUser.Name);
        Assert.AreEqual("Doe", loggedUser.LastName);
        Assert.AreEqual("john.doe@example.com", loggedUser.Email);
    }
}