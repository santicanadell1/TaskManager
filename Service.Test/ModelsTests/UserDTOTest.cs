using Domain;
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
    
    [TestMethod]
    public void NewUser_WhenPasswordIsNull_ThenUserIsNotCreated()
    {
        // arrange
        UserDTO user = new UserDTO { Password = null };

        // act & assert
        Assert.IsNull(user.Password);
    }
    
    [TestMethod]
    public void NewUser_WhenRolesAreAssigned_ThenRolesAreSet()
    {
        // arrange
        var roles = new List<Rol> { Rol.AdminSystem, Rol.ProjectMember };
        var user = new UserDTO { Roles = roles };

        // act & assert
        Assert.IsNotNull(user.Roles);
        Assert.AreEqual(2, user.Roles.Count);
        Assert.IsTrue(user.Roles.Contains(Rol.AdminSystem));
        Assert.IsTrue(user.Roles.Contains(Rol.ProjectMember));
    }

    [TestMethod]
    public void NewUser_WhenTasksAreAssigned_ThenTasksAreSet()
    {
        List<int> tasksIds = new List<int> { 1, 2, 3 };
        var user = new UserDTO{Tasks = tasksIds};
        Assert.IsNotNull(user.Tasks);
    }

    
}