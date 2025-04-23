namespace Domain.Test;

[TestClass]
public class UserTest
{
    [TestMethod]
    public void NewUser_WhenConstructorIsNotEmpty_ThenUserIsCreated()
    {
        // Arrange
        User user;
        DateTime birthday = DateTime.Parse("10/05/2005");
        //act
        user = new User("First Name", "Last Name", "Email", birthday, "Password");
        //assert
        Assert.IsNotNull(user);
    }
}