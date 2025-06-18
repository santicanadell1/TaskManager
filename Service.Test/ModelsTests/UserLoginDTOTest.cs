using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class UserLoginDTOTest
{
    [TestMethod]
    public void UserLoginDTO_WhenValidEmailAndPassword_ThenObjectIsCreated()
    {
        String validEmail = "test@email.com";
        String validPassword = "ValidPassword123";

        UserLoginDTO userLogin = new UserLoginDTO(validEmail, validPassword);

        Assert.IsNotNull(userLogin);
        Assert.AreEqual(validEmail, userLogin.Email);
        Assert.AreEqual(validPassword, userLogin.Password);
    }
}