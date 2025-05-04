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

        var addedResource = _database.resources.Get(r => r.Name == "Resource1");

        var resource = _resourceService.Get(addedResource.Id);

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

    [TestMethod]
    public void UpdateResource_ShouldUpdateResource_WhenResourceExists()
    {
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = "Description of Resource1",
        };

        _resourceService.AddResource(resourceDTO);

        var addedResource = _database.resources.Get(r => r.Name == "Resource1");

        var updatedResourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeB",
            Description = "Updated description"
        };

        _resourceService.UpdateResource(addedResource.Id, updatedResourceDTO);

        var resource = _database.resources.Get(r =>
            r.Name == updatedResourceDTO.Name && r.Type == updatedResourceDTO.Type &&
            r.Description == updatedResourceDTO.Description);

        Assert.IsNotNull(resource);
        Assert.AreEqual("TypeB", resource.Type);
        Assert.AreEqual("Updated description", resource.Description);
    }

    [TestMethod]
    public void DeleteResource_ShouldDeleteResource_WhenResourceExists()
    {
        var resourceDTO = new ResourceDTO
        {
            Name = "ResourceToDelete",
            Type = "TypeA",
            Description = "Description of resource to delete"
        };

        _resourceService.AddResource(resourceDTO);

        var addedResource = _database.resources.Get(r => r.Name == resourceDTO.Name);
        Assert.IsNotNull(addedResource);

        _resourceService.DeleteResource(addedResource.Id);

        var deletedResource = _database.resources.Get(r => r.Name == resourceDTO.Name);
        Assert.IsNull(deletedResource);
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceNotFoundException))]
    public void DeleteResource_ShouldThroException_WhenResourceNoExists()
    {
        _resourceService.DeleteResource(999);
    }
}