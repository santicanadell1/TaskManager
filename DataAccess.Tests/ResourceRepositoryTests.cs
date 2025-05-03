using DataAccess.ResourceRepositoryExceptions;
using Domain;

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
    
    [TestMethod]
    public void AddResource_ShouldAddResource_WhenValidResourceIsProvided()
    {
        var resource = new Resource("Resource1", "TypeA", "Description of Resource1");

        _resourceRepository.AddResource(resource);

        var addedResource = _resourceRepository.Get(r => r.Name == resource.Name);
        Assert.IsNotNull(addedResource);
        Assert.AreEqual(resource.Name, addedResource?.Name);
    }
    
    
    [TestMethod]
    public void Update_ShouldUpdateResource_WhenResourceExists()
    {
        var resource = new Resource("Resource1", "TypeA", "Description of Resource1");
        _resourceRepository.AddResource(resource);

        var updatedResource = new Resource("Resource1", "TypeB", "Updated Description");
        _resourceRepository.Update(resource.Id, updatedResource);

        var result = _resourceRepository.Get(r => r.Name == updatedResource.Name);
        Assert.IsNotNull(result);
        Assert.AreEqual(updatedResource.Type, result?.Type);
        Assert.AreEqual(updatedResource.Description, result?.Description);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ResourceNotFoundException))]
    public void Delete_ShouldThrowException_WhenResourceDoesNotExist()
    {
        _resourceRepository.Delete(999);  
    }
    
}