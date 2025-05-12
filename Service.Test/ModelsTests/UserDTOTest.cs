using Domain;
using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class UserDTOTest
{
    [TestMethod]
    public void NewUser_WhenFirstNameIsNull_ThenUserIsNotCreated()
    {
        UserDTO user = new UserDTO { FirstName = null };

        Assert.IsNull(user.FirstName);
    }
    
    [TestMethod]
    public void NewUser_WhenLastNameIsNull_ThenUserIsNotCreated()
    {
        UserDTO user = new UserDTO { LastName = null };

        Assert.IsNull(user.LastName);
    }
    
    [TestMethod]
    public void NewUser_WhenEmailIsNull_ThenUserIsNotCreated()
    {
        UserDTO user = new UserDTO { Email = null };

        Assert.IsNull(user.Email);
    }
    
    [TestMethod]
    public void NewUser_WhenBirthdayIsDefault_ThenUserIsNotCreated()
    {
        UserDTO user = new UserDTO { Birthday = default(DateTime) };

        Assert.AreEqual(default(DateTime), user.Birthday);
    }
    
    [TestMethod]
    public void NewUser_WhenPasswordIsNull_ThenUserIsNotCreated()
    {
        UserDTO user = new UserDTO { Password = null };

        Assert.IsNull(user.Password);
    }
    
    [TestMethod]
    public void NewUser_WhenRolesAreAssigned_ThenRolesAreSet()
    {
        var roles = new List<RolDTO> { RolDTO.AdminSystem, RolDTO.ProjectMember };
        var user = new UserDTO { Roles = roles };

        Assert.IsNotNull(user.Roles);
        Assert.AreEqual(2, user.Roles.Count);
        Assert.IsTrue(user.Roles.Contains(RolDTO.AdminSystem));
        Assert.IsTrue(user.Roles.Contains(RolDTO.ProjectMember));
    }

    [TestMethod]
    public void NewUser_WhenTasksAreAssigned_ThenTasksAreSet()
    {
        List<int> tasksIds = new List<int> { 1, 2, 3 };
        var user = new UserDTO{Tasks = tasksIds};
        Assert.IsNotNull(user.Tasks);
    }

    
}