using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class GanttLinkTest
{
    [TestMethod]
    public void GanttLink_WhenInitializing_ShouldInitializeWithDefaultType()
    {
        var link = new GanttLink();
        var type = link.type;
        Assert.AreEqual("0", type);
    }

    [TestMethod]
    public void GanttLink_WhenInitializingCritical_ShouldBeFalseByDefault()
    {
        var link = new GanttLink();
        var critical = link.critical;
        Assert.IsFalse(critical);
    }

    [TestMethod]
    public void GanttLink_WhenInitializingAType_ShouldAllowCustomValue()
    {
        var link = new GanttLink { type = "2" };
        var type = link.type;
        Assert.AreEqual("2", type);
    }

    [TestMethod]
    public void GanttLink_ShouldSetAllPropertiesCorrectly()
    {
        var link = new GanttLink
        {
            id = 1,
            source = 101,
            target = 202,
            type = "1",
            critical = true
        };
        Assert.AreEqual(1, link.id);
        Assert.AreEqual(101, link.source);
        Assert.AreEqual(202, link.target);
        Assert.AreEqual("1", link.type);
        Assert.IsTrue(link.critical);
    }
}