using Service.Models;

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
    
    [TestMethod]
    public void NewUser_WhenLastNameIsNull_ThenUserIsNotCreated()
    {
        // arrange
        UserDTO user = new UserDTO { LastName = null };

        // act & assert
        Assert.IsNull(user.LastName);
    }
    
    [TestMethod]
    public void NewUser_WhenEmailIsNull_ThenUserIsNotCreated()
    {
        // arrange
        UserDTO user = new UserDTO { Email = null };

        // act & assert
        Assert.IsNull(user.Email);
    }
    
    [TestMethod]
    public void NewUser_WhenBirthdayIsDefault_ThenUserIsNotCreated()
    {
        // arrange
        UserDTO user = new UserDTO { Birthday = default(DateTime) };

        // act & assert
        Assert.AreEqual(default(DateTime), user.Birthday);
    }
}