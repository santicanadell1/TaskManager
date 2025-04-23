namespace Service.Test;

[TestClass]
public class PasswordMTest
{
    [TestMethod]
    public void DefaultPassword_ShouldBeValid()
    {
        // Arrange
        PasswordManager passwordManager = new PasswordManager();  // default password is set in the constructor

        // Act
        bool isValid = passwordManager.IsValidPassword(passwordManager.Password);

        // Assert
        Assert.IsTrue(isValid, "The default password should be valid.");
    }
}