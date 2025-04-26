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
    
}