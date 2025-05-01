namespace Domain.Test;

[TestClass]
public class UserTest
{
    [TestMethod]
    public void NewUser_WhenConstructorIsNotEmpty_ThenUserIsCreated()
    {
        
        User user;
        DateTime birthday = DateTime.Parse("10/05/2005");
       
        user = new User("First Name", "Last Name", "Email@email.com", birthday, "Password");
       
        Assert.IsNotNull(user);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenFirstNameIsNull_ThenThrowArgumentException()
    {
       
        User user ;
        DateTime birthday = DateTime.Parse("10/05/2005");
      
        user = new User("  ", "Last Name", "Email@email.com", birthday, "Password");
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenLastNameIsNull_ThenThrowArgumentException()
    {
        
        User user ;
        DateTime birthday = DateTime.Parse("10/05/2005");
       
        user = new User("First Name", "", "Email@email.com", birthday, "Password");
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenEmailIsNull_ThenThrowArgumentException()
    {
        
        User user ;
        DateTime birthday = DateTime.Parse("10/05/2005");
        
        user = new User("First Name", "Last Name", "", birthday, "Password");
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenEmailHasAnInvalidFormat_ThenThrowArgumentException()
    {
       
        User user ;
        DateTime birthday = DateTime.Parse("10/05/2005");
        
        user = new User("First Name", "Last Name", "email", birthday, "Password");
    }
    [TestMethod]
    public void NewUser_WhenEmailIsValid_ThenUserIsCreated()
    {
        
        User user;
        DateTime birthday = DateTime.Parse("10/05/2005");
        
        user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");
       
        Assert.IsNotNull(user);
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenDateIsAfterToday_ThenThrowArgumentException()
    {
        
        User user ;
        DateTime birthday = DateTime.Parse("20/07/2026");
        
        user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewUser_WhenPasswordIsNull_ThenThrowArgumentException()
    {
        
        User user ;
        DateTime birthday = DateTime.Parse("10/05/2005");
        
        user = new User("First Name", "Last Name", "Email@email.com", birthday, "");
    }
    
    [TestMethod]
    public void AddRol_WhenRoleIsAdded_ThenRoleIsInList()
    {
       
        var roles = new List<Rol> { Rol.AdminSystem };
        User user;
        DateTime birthday = DateTime.Parse("10/05/2005");
        user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");

        
        user.Roles = roles;
        user.AddRol(Rol.ProjectMember);

        
        Assert.AreEqual(2, user.Roles.Count);
        Assert.IsTrue(user.Roles.Contains(Rol.ProjectMember));
    }
    
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AddRol_WhenRoleAlreadyExists_ThenThrowInvalidOperationException()
    {
        
        var roles = new List<Rol> { Rol.AdminSystem };
        User user;
        DateTime birthday = DateTime.Parse("10/05/2005");
        user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");

        
        user.Roles = roles;
        user.AddRol(Rol.AdminSystem); 

    }
    
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void RemoveRol_WhenRoleDoesNotExist_ThenThrowInvalidOperationException()
    {
        
        var roles = new List<Rol> { Rol.AdminSystem, Rol.AdminProject};
        User user;
        DateTime birthday = DateTime.Parse("10/05/2005");
        user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");
        
     
        user.Roles = roles;
        user.RemoveRol(Rol.AdminProject);
        user.RemoveRol(Rol.ProjectMember);

    }
    
    [TestMethod]
    public void User_WhenInitializedWithEmptyConstructor_ThenPropertiesAreInitializedWithDefaultValues()
    {
       
        User user;

      
        user = new User();
        
    }
    
    
}