using Service.Models;

namespace Service.Test.ModelsTests;


[TestClass]
public class UserLoginDTOTest
{
    [TestMethod]
    public void UserLoginDTO_WhenValidEmailAndPassword_ThenObjectIsCreated()
    {
        // arrange
        var validEmail = "test@email.com";
        var validPassword = "ValidPassword123";

        // act
        var userLogin = new UserLoginDTO(validEmail, validPassword);

        // assert
        Assert.IsNotNull(userLogin);
        Assert.AreEqual(validEmail, userLogin.Email);
        Assert.AreEqual(validPassword, userLogin.Password);
    }
}