using Domain.Exceptions.ResourceExceptions;

namespace Domain.Test;

[TestClass]
public class ResourceTest
{
    [TestMethod]
    [ExpectedException(typeof(ResourceNameException))]
    public void NewResource_WhenNameIsEmpty_ThenThrowResourceNameException()
    {
        Resource res;

        res = new Resource("", "type", "description");
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceTypeException))]
    public void NewResource_WhenTypeIsEmpty_ThenThrowResourceTypeException()
    {
        Resource res;

        res = new Resource("name", "", "description");
    }

    [TestMethod]
    public void NewResource_WithValidValues_ShouldCreateResourceCorrectly()
    {
        var name = "Laptop";
        var type = "Hardware";
        var description = "Dell Latitude";


        var res = new Resource(name, type, description);


        Assert.AreEqual(name, res.Name);
        Assert.AreEqual(type, res.Type);
        Assert.AreEqual(description, res.Description);
    }

    [TestMethod]
    public void UpdateResourceName_ShouldChangeNameSuccessfully()
    {
        var res = new Resource("Old Name", "Humano", "Some description");


        res.Name = "New Name";
        res.Id = 1;


        Assert.AreEqual("New Name", res.Name);
    }

    [TestMethod]
    public void SetConcurrentUsage_ShouldSetConcurrentUsageCorrectly()
    {
        var res = new Resource("Name", "Type", "Some description");
        res.ConcurrentUsage = true;

        Assert.IsTrue(res.ConcurrentUsage);
    }

    [TestMethod]
    public void SetConcurrentUsage_ShouldSetConcurrentUsageFalseByDefault()
    {
        var res = new Resource("Name", "Type", "Description");
        Assert.IsFalse(res.ConcurrentUsage);
    }

    [TestMethod]
    public void SetConcurrentUsage_WhenConcurrentUsageIsSetToTrue_ShouldSetConcurrentUsageToTrue()
    {
        var res = new Resource("Name", "Type", "Description");
        res.ConcurrentUsage = true;

        Assert.IsTrue(res.ConcurrentUsage);
    }

    [TestMethod]
    public void CreateResource_WithProject_ThenResourceShouldBeCreated()
    {
        var project = new Project();
        project.Name = "Project1";
        var resource = new Resource("Resource1", "TypeA", "Description of Resource1", false, project);
        Assert.IsNotNull(resource.Project);
    }
}