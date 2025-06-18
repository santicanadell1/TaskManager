using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;

namespace DataAccess.Test;

[TestClass]
public class ResourceRepositoryTests
{
    private AppDbContext _context;
    private InMemoryAppContextFactory _contextFactory;
    private ResourceRepository _resourceRepository;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryAppContextFactory();
        _context = _contextFactory.CreateDbContext();
        _resourceRepository = new ResourceRepository(_context);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context.Database.EnsureDeleted();
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

        _resourceRepository.Add(resource);

        var addedResource = _resourceRepository.Get(r => r.Name == resource.Name);
        Assert.IsNotNull(addedResource);
        Assert.AreEqual(resource.Name, addedResource?.Name);
    }

    [TestMethod]
    public void Update_ShouldUpdateResource_WhenResourceExists()
    {
        var resource = new Resource("Resource1", "TypeA", "Description of Resource1");
        _resourceRepository.Add(resource);

        var updatedResource = new Resource("Resource1.v1", "TypeB", "Updated Description");
        updatedResource.Id = _resourceRepository.Get(r => r.Name == resource.Name).Id;
        _resourceRepository.Update(updatedResource);

        var result = _resourceRepository.Get(r =>
            r.Name == updatedResource.Name && r.Description == updatedResource.Description &&
            r.Type == updatedResource.Type);

        Assert.IsNotNull(result);
        Assert.AreEqual(updatedResource.Type, result?.Type);
        Assert.AreEqual(updatedResource.Description, result?.Description);
        Assert.AreEqual(updatedResource.Name, result?.Name);
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceNotFoundException))]
    public void Delete_ShouldThrowException_WhenResourceDoesNotExist()
    {
        _resourceRepository.Delete(new Resource());
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceIsNullException))]
    public void Delete_ShouldThrowException_WhenAddingNullResourceAfterDeletion()
    {
        var resource = new Resource("Resource1", "TypeA", "Description of Resource1");
        _resourceRepository.Add(resource);
        resource.Id = _resourceRepository.Get(r => r.Name == resource.Name).Id;

        _resourceRepository.Delete(resource);

        var deletedResource = _resourceRepository.Get(r => r.Name == resource.Name);

        _resourceRepository.Add(deletedResource);
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceIsNullException))]
    public void Add_ShouldThrowResourceIsNullException_WhenResourceIsNull()
    {
        _resourceRepository.Add(null);
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceNotFoundException))]
    public void Update_ShouldThrowResourceNotFoundException_WhenResourceIsNull()
    {
        _resourceRepository.Update(null);
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceNotFoundException))]
    public void Update_ShouldThrowResourceNotFoundException_WhenResourceDoesNotExist()
    {
        var nonExistentResource = new Resource("NonExistent", "Type", "Description");
        nonExistentResource.Id = 999;

        _resourceRepository.Update(nonExistentResource);
    }

    [TestMethod]
    public void Add_ShouldSuccessfullyAddResource_WhenValidResourceProvided()
    {
        var resource = new Resource("ValidResource", "ValidType", "Valid Description");

        _resourceRepository.Add(resource);

        var retrievedResource = _resourceRepository.Get(r => r.Name == "ValidResource");
        Assert.IsNotNull(retrievedResource);
        Assert.AreEqual("ValidResource", retrievedResource.Name);
        Assert.AreEqual("ValidType", retrievedResource.Type);
        Assert.AreEqual("Valid Description", retrievedResource.Description);
    }


    [TestMethod]
    public void Update_ShouldSuccessfullyUpdateAllFields_WhenResourceExists()
    {
        var originalResource = new Resource("Original", "OriginalType", "Original Description");
        _resourceRepository.Add(originalResource);

        var addedResource = _resourceRepository.Get(r => r.Name == "Original");
        var updatedResource = new Resource("Updated", "UpdatedType", "Updated Description");
        updatedResource.Id = addedResource.Id;

        _resourceRepository.Update(updatedResource);

        var retrievedResource = _resourceRepository.Get(r => r.Id == updatedResource.Id);
        Assert.IsNotNull(retrievedResource);
        Assert.AreEqual("Updated", retrievedResource.Name);
        Assert.AreEqual("UpdatedType", retrievedResource.Type);
        Assert.AreEqual("Updated Description", retrievedResource.Description);
    }
}