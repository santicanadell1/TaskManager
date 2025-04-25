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
        //Assert
        Assert.IsNull(res);
        
        
    }
    
    
}