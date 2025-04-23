using Service.Passsword;
namespace Service.Test;

[TestClass]
public class PasswordMTest
{
    [TestMethod]
    public void DefaultPassword_ShouldBeValid()
    {
        // Act
        bool isValid = PasswordManager.IsValidPassword(PasswordManager.getDefaultPassword());

        // Assert
        Assert.IsTrue(isValid, "The default password should be valid.");
    }
    
    [TestMethod]
    public void HashPassword_ShouldReturnSameHashForSamePassword()
    {
        // Arrange
        string password1 = "FNSabc1?";
        string password2 = "FNSabc1?";

        // Act
        string hash1 = PasswordManager.HashPassword(password1);
        string hash2 = PasswordManager.HashPassword(password2);

        // Assert
        Assert.AreEqual(hash1, hash2, "The hash should be the same for the same password.");
    }
    
    [TestMethod]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        string originalPassword = "FNSabc1?";
        string wrongPassword = "Incorrect1?";  // Incorrect password
        string hashedPassword = PasswordManager.HashPassword(originalPassword);  // Hash the original password

        // Act: Simulate unhashing with the wrong password
        bool isPasswordValid = PasswordManager.VerifyPassword(wrongPassword, hashedPassword);

        // Assert: The wrong password should not match the hash
        Assert.IsFalse(isPasswordValid, "The wrong password should not match the hash.");
    }
    
}