using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class GanttLinkTest
{
    [TestMethod]
    public void GanttLink_WhenInitializing_ShouldInitializeWithDefaultType()
    {
        // Arrange
        var link = new GanttLink();

        // Act
        var type = link.type;

        // Assert
        Assert.AreEqual("0", type);
    }
    [TestMethod]
    public void GanttLink_WhenInitializingCritical_ShouldBeFalseByDefault()
    {
        // Arrange
        var link = new GanttLink();

        // Assert
        Assert.IsFalse(link.critical);
    }
    [TestMethod]
    public void GanttLink_WhenInitializingAType_ShouldAllowCustomValue()
    {
        // Arrange
        var link = new GanttLink { type = "2" };

        // Assert
        Assert.AreEqual("2", link.type);
    }
    
    [TestMethod]
    public void GanttLink_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var link = new GanttLink
        {
            id = 1,
            source = 101,
            target = 202,
            type = "1",
            critical = true
        };

        // Assert
        Assert.AreEqual(1, link.id);
        Assert.AreEqual(101, link.source);
        Assert.AreEqual(202, link.target);
        Assert.AreEqual("1", link.type);
        Assert.IsTrue(link.critical);
    }
}
