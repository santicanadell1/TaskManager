using DataAccess;
using Service.Models;

namespace Service.Test;

[TestClass]
public class ResourcesServiceTest
{
    private InMemoryDatabase _database;
    private ResourceService _resourceService;

    [TestInitialize]
    public void SetUp()
    {
        _database = new InMemoryDatabase();
        _resourceService = new ResourceService(_database);
    }
    
    [TestMethod]
    public void AddResource_ShouldAddResource_WhenValidDTO()
    {
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);

        var resource = _database.resources.Get(r => r.Name == "Resource1");
        Assert.IsNotNull(resource);  
        Assert.AreEqual("Resource1", resource.Name);  
        Assert.AreEqual("TypeA", resource.Type);  
        Assert.AreEqual("Description of Resource1", resource.Description);  
    }
}