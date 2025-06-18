namespace Service.Test;

[TestClass]
public class PasswordMTest
{
    [TestMethod]
    public void DefaultPassword_ShouldBeValid()
    {
        var passwordManager = new PasswordManager();

        var isValid = passwordManager.IsValidPassword(passwordManager.getDefaultPassword());

        Assert.IsTrue(isValid, "The default password should be valid.");
    }

    [TestMethod]
    public void HashPassword_ShouldReturnSameHashForSamePassword()
    {
        var passwordManager = new PasswordManager();
        var password1 = "FNSabc1?";
        var password2 = "FNSabc1?";

        var hash1 = passwordManager.HashPassword(password1);
        var hash2 = passwordManager.HashPassword(password2);

        Assert.AreEqual(hash1, hash2, "The hash should be the same for the same password.");
    }

    [TestMethod]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        var passwordManager = new PasswordManager();
        var originalPassword = "FNSabc1?";
        var wrongPassword = "Incorrect1?";
        var hashedPassword = passwordManager.HashPassword(originalPassword);

        var isPasswordValid = passwordManager.VerifyPassword(wrongPassword, hashedPassword);

        Assert.IsFalse(isPasswordValid, "The wrong password should not match the hash.");
    }
}