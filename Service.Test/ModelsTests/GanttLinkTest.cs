using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class GanttLinkTest
{
    [TestMethod]
    public void GanttLink_WhenInitializing_ShouldInitializeWithDefaultType()
    {
        GanttLink link = new GanttLink();
        string type = link.type;
        Assert.AreEqual("0", type);
    }

    [TestMethod]
    public void GanttLink_WhenInitializingCritical_ShouldBeFalseByDefault()
    {
        GanttLink link = new GanttLink();
        bool critical = link.critical;
        Assert.IsFalse(critical);
    }

    [TestMethod]
    public void GanttLink_WhenInitializingAType_ShouldAllowCustomValue()
    {
        GanttLink link = new GanttLink { type = "2" };
        string type = link.type;
        Assert.AreEqual("2", type);
    }

    [TestMethod]
    public void GanttLink_ShouldSetAllPropertiesCorrectly()
    {
        GanttLink link = new GanttLink
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