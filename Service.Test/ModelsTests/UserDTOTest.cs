namespace Service.Test.ModelsTests;

[TestClass]
public class UserDTOTest
{
    [TestMethod]
    public void NewUser_WhenFirstNameIsNull_ThenUserIsNotCreated()
    {
        // arrange
        UserDTO user = new UserDTO { FirstName = null };

        // act & assert
        Assert.IsNull(user.FirstName);
    }
}