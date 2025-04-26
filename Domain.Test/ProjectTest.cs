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

    
    
    
}