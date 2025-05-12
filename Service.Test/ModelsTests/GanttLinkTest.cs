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
    throw new NotImplementedException();
    }
}
