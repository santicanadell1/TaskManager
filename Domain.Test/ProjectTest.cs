namespace Domain.Test;
[TestClass]
public class ProjectTest
{
    [TestMethod]
    public void GivenProject_WhenNameIsSet_ThenNameShouldBeCorrect()
    {
        // Arrange
        var project = new Project();
        string expectedName = "Project A";
            
        // Act
        project.Name = expectedName;
        string actualName = project.Name;

        // Assert
        Assert.AreEqual(expectedName, actualName);
    }
    
    [TestMethod]
    public void GivenProject_WhenDescriptionIsSet_ThenDescriptionShouldBeCorrect()
    {
        // Arrange
        var project = new Project();
        string expectedDescription = "This is a test project";
            
        // Act
        project.Description = expectedDescription;
        string actualDescription = project.Description;

        // Assert
        Assert.AreEqual(expectedDescription, actualDescription);
    }
    
    [TestMethod]
    public void GivenProject_WhenStartDateIsSet_ThenStartDateShouldBeCorrect()
    {
        // Arrange
        var project = new Project();
        DateTime expectedStartDate = new DateTime(2025, 5, 1);
            
        // Act
        project.StartDate = expectedStartDate;
        DateTime actualStartDate = project.StartDate;

        // Assert
        Assert.AreEqual(expectedStartDate, actualStartDate);
    }
    
    
    [TestMethod]
    public void GivenProject_WhenMembersAreSet_ThenMembersShouldBeCorrect()
    {
        // Arrange
        var project = new Project();
        var expectedMembers = new List<User>();
            
        // Act
        project.Members = expectedMembers;
        List<User> actualMembers = project.Members;

        // Assert
        CollectionAssert.AreEqual(expectedMembers, actualMembers);
    }
    
    [TestMethod]
    public void GivenTaskList_WhenTasksAreAdded_ThenTaskListShouldContainCorrectTasks()
    {
        // Arrange
        var project = new Project();
        var expectedTasks = new List<Task>();
        
        // Act
        project.Tasks = expectedTasks; 
        var actualTasks = project.Tasks; 

        // Assert
        CollectionAssert.AreEqual(expectedTasks, actualTasks); 
    }

    [TestMethod]
    public void GivenProject_WhenAdminProjectIsSet_ThenAdminProjectShouldBeCorrect()
    {
        // Arrange
        var project = new Project();
        var adminUser = new User();
    
        adminUser.Roles = new List<Rol>(); 
    
        adminUser.AddRol(Rol.AdminProject);  

        // Act
        project.AdminProject = adminUser;
        var actualAdminProject = project.AdminProject;

        // Assert
        Assert.IsTrue(actualAdminProject.Roles.Contains(Rol.AdminProject)); 
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void GivenProject_WhenNameIsNull_ThenArgumentExceptionShouldBeThrown()
    {
        // Arrange
        var project = new Project();
    
        // Act
        project.Name = null; 
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void GivenProject_WhenDescriptionIsEmpty_ThenArgumentExceptionShouldBeThrown()
    {
        // Arrange
        var project = new Project();
    
        // Act
        project.Description = "";  
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void GivenProject_WhenStartDateIsDefault_ThenArgumentExceptionShouldBeThrown()
    {
        // Arrange
        var project = new Project();
    
        // Act
        project.StartDate = default(DateTime);  
    }
    
  

    

    
}