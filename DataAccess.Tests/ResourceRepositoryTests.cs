namespace DataAccess.Test;

[TestClass]
public class ResourceRepositoryTests
{
    private ResourceRepository _resourceRepository;
    [TestInitialize]
    public void Setup()
    {
        _resourceRepository = new ResourceRepository();
    }

    [TestMethod]
    public void GetAll_ShouldReturnEmptyList_WhenNoResourcesAdded()
    {
        var resources = _resourceRepository.GetAll();
        Assert.AreEqual(0, resources.Count);
    }
    
}