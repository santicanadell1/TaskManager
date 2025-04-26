namespace Domain.Test;

[TestClass]
public class UserTest
{
    [TestMethod]
    public void NewUser_WhenConstructorIsNotEmpty_ThenUserIsCreated()
    {
        // arrange
        User user;
        DateTime birthday = DateTime.Parse("10/05/2005");
        //act
        user = new User("First Name", "Last Name", "Email@email.com", birthday, "Password");
        //assert
        Assert.IsNotNull(user);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenFirstNameIsNull_ThenThrowArgumentException()
    {
        //arrange
        User user ;
        DateTime birthday = DateTime.Parse("10/05/2005");
        //act
        user = new User("  ", "Last Name", "Email@email.com", birthday, "Password");
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenLastNameIsNull_ThenThrowArgumentException()
    {
        //arrange
        User user ;
        DateTime birthday = DateTime.Parse("10/05/2005");
        //act
        user = new User("First Name", "", "Email@email.com", birthday, "Password");
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenEmailIsNull_ThenThrowArgumentException()
    {
        //arrange
        User user ;
        DateTime birthday = DateTime.Parse("10/05/2005");
        //act
        user = new User("First Name", "Last Name", "", birthday, "Password");
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenEmailHasAnInvalidFormat_ThenThrowArgumentException()
    {
        //arrange
        User user ;
        DateTime birthday = DateTime.Parse("10/05/2005");
        //act
        user = new User("First Name", "Last Name", "email", birthday, "Password");
    }
    [TestMethod]
    public void NewUser_WhenEmailIsValid_ThenUserIsCreated()
    {
        // arrange
        User user;
        DateTime birthday = DateTime.Parse("10/05/2005");
        //act
        user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");
        //assert
        Assert.IsNotNull(user);
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenDateIsAfterToday_ThenThrowArgumentException()
    {
        //arrange
        User user ;
        DateTime birthday = DateTime.Parse("20/07/2026");
        //act
        user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenPasswordIsNull_ThenThrowArgumentException()
    {
        //arrange
        User user ;
        DateTime birthday = DateTime.Parse("10/05/2005");
        //act
        user = new User("First Name", "Last Name", "Email@email.com", birthday, "");
    }
    
    [TestMethod]
    public void AddRol_WhenRoleIsAdded_ThenRoleIsInList()
    {
        // Arrange
        var roles = new List<Rol> { Rol.AdminSystem };
        User user;
        DateTime birthday = DateTime.Parse("10/05/2005");
        user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");

        // Act
        user.Roles = roles;
        user.AddRol(Rol.ProjectMember);

        // Assert
        Assert.AreEqual(2, user.Roles.Count);
        Assert.IsTrue(user.Roles.Contains(Rol.ProjectMember));
    }
    
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AddRol_WhenRoleAlreadyExists_ThenThrowInvalidOperationException()
    {
        // Arrange
        var roles = new List<Rol> { Rol.AdminSystem };
        User user;
        DateTime birthday = DateTime.Parse("10/05/2005");
        user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");

        // Act
        user.Roles = roles;
        user.AddRol(Rol.AdminSystem); 

    }
    
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void RemoveRol_WhenRoleDoesNotExist_ThenThrowInvalidOperationException()
    {
        // Arrange: 
        var roles = new List<Rol> { Rol.AdminSystem };
        User user;
        DateTime birthday = DateTime.Parse("10/05/2005");
        user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");
        
        // Act: 
        user.Roles = roles;
        user.RemoveRol(Rol.ProjectMember);

    }
    
    
}