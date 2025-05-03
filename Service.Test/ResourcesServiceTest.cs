using DataAccess;
using DataAccess.ResourceRepositoryExceptions;
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

    [TestMethod]
    public void Get_ShouldReturnResource_WhenResourceExists()
    {
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        _resourceService.AddResource(resourceDTO);
        
        var resource = _resourceService.Get(1);

        Assert.IsNotNull(resource);
        Assert.AreEqual("Resource1", resource.Name);
        Assert.AreEqual("TypeA", resource.Type);
        Assert.AreEqual("Description of Resource1", resource.Description);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ResourceNotFoundException))]
    public void Get_ShouldThrowException_WhenResourceDoesNotExist()
    {
        _resourceService.Get(999); 
    }
    
    [TestMethod]
    public void GetResources_ShouldReturnAllResources_WhenResourcesExist()
    {
        var resourceDTO1 = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1"
        };

        var resourceDTO2 = new ResourceDTO
        {
            Name = "Resource2",
            Type = "TypeB",
            Description = "Description of Resource2"
        };

        _resourceService.AddResource(resourceDTO1);
        _resourceService.AddResource(resourceDTO2);

        var resources = _resourceService.GetResources();

        Assert.AreEqual(2, resources.Count);  
        Assert.IsTrue(resources.Exists(r => r.Name == "Resource1"));  
        Assert.IsTrue(resources.Exists(r => r.Name == "Resource2"));  
    }
}