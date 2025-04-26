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

    
    
    
}