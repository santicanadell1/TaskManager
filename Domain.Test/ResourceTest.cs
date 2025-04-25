namespace Domain.Test;

[TestClass]
public class ResourceTest
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewResource_WhenNameIsEmpty_ThenThrowException()
    {
        //Arrange
        Resource res;
        //Act
        res = new Resource("","type","description");
        
      
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewResource_WhenTypeIsEmpty_ThenThrowException()
    {
        //Arrange
        Resource res;
        //Act
        res = new Resource("name","","description");
        
        
    }
    
    [TestMethod]
    public void NewResource_WithValidValues_ShouldCreateResourceCorrectly()
    {
        // Arrange
        var name = "Laptop";
        var type = "Hardware";
        var description = "Dell Latitude";

        // Act
        var res = new Resource(name, type, description);

        // Assert
        Assert.AreEqual(name, res.Name);
        Assert.AreEqual(type, res.Type);
        Assert.AreEqual(description, res.Description);
    }
    
}