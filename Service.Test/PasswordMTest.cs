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
}