namespace Service.Test;

[TestClass]
public class PasswordMTest
{
    [TestMethod]
    public void DefaultPassword_ShouldBeValid()
    {
        PasswordManager passwordManager = new PasswordManager();
        
        bool isValid = passwordManager.IsValidPassword(passwordManager.getDefaultPassword());

        Assert.IsTrue(isValid, "The default password should be valid.");
    }
    
    [TestMethod]
    public void HashPassword_ShouldReturnSameHashForSamePassword()
    {
        PasswordManager passwordManager = new PasswordManager();
        string password1 = "FNSabc1?";
        string password2 = "FNSabc1?";

        string hash1 = passwordManager.HashPassword(password1);
        string hash2 = passwordManager.HashPassword(password2);

        Assert.AreEqual(hash1, hash2, "The hash should be the same for the same password.");
    }
    
    [TestMethod]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        PasswordManager passwordManager = new PasswordManager();
        string originalPassword = "FNSabc1?";
        string wrongPassword = "Incorrect1?";  // Incorrect password
        string hashedPassword = passwordManager.HashPassword(originalPassword);  // Hash the original password

        bool isPasswordValid = passwordManager.VerifyPassword(wrongPassword, hashedPassword);

        Assert.IsFalse(isPasswordValid, "The wrong password should not match the hash.");
    }
    
}