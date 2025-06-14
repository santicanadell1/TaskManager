using DataAccess.Exceptions.ResourceRepositoryExceptions;
using Domain;

namespace DataAccess.Test;

[TestClass]
public class ResourceRepositoryTests
{
    private ResourceRepository _resourceRepository;
    private AppDbContext _context;
    private InMemoryAppContextFactory _contextFactory;

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
        List<Resource> resources = _resourceRepository.GetAll();
        Assert.AreEqual(0, resources.Count);
    }

    [TestMethod]
    public void AddResource_ShouldAddResource_WhenValidResourceIsProvided()
    {
        Resource resource = new Resource("Resource1", "TypeA", "Description of Resource1");

        _resourceRepository.Add(resource);

        Resource addedResource = _resourceRepository.Get(r => r.Name == resource.Name);
        Assert.IsNotNull(addedResource);
        Assert.AreEqual(resource.Name, addedResource?.Name);
    }

    [TestMethod]
    public void Update_ShouldUpdateResource_WhenResourceExists()
    {
        Resource resource = new Resource("Resource1", "TypeA", "Description of Resource1");
        _resourceRepository.Add(resource);

        Resource updatedResource = new Resource("Resource1.v1", "TypeB", "Updated Description");
        updatedResource.Id = _resourceRepository.Get(r => r.Name == resource.Name).Id;
        _resourceRepository.Update(updatedResource);

        Resource result = _resourceRepository.Get(r =>
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
        Resource resource = new Resource("Resource1", "TypeA", "Description of Resource1");
        _resourceRepository.Add(resource);
        resource.Id = _resourceRepository.Get(r => r.Name == resource.Name).Id;

        _resourceRepository.Delete(resource);

        Resource deletedResource = _resourceRepository.Get(r => r.Name == resource.Name);

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
        _resourceRepository(null);
    }

    
}