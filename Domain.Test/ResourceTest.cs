namespace Domain.Test;

[TestClass]
public class ResourceTest
{
    [TestMethod]
    public void NewResource_WhenNameIsEmpty_ThenThrowException()
    {
        //Arrange
        Resource res;
        //Act
        res = new Resource("","type","description");
        //Assert
        Assert.IsNull(res);
    }
}