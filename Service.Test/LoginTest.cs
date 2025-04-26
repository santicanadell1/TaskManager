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

        // Add a user to the repository for testing
        _userRepository.Add(new User
        {
            Email = email,
            Name = "John",
            LastName = "Doe",
            Password = password, // Correct password
            Role = "Admin System"
        });

        // Act
        _login.Login(email, password);

        // Assert
        var loggedUser = LoggedUser.Current;
        Assert.IsNotNull(loggedUser);
        Assert.AreEqual("John", loggedUser.Name);
        Assert.AreEqual("Doe", loggedUser.LastName);
        Assert.AreEqual("john.doe@example.com", loggedUser.Email);
    }
}