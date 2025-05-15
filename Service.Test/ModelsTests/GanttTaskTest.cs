using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class GanttTaskTests
{
    [TestMethod]
    public void GanttTask_WhenInitializing_ShouldAllowNonCriticalTasks()
    {
        var task = new GanttTask { critical = false };
        Assert.IsFalse(task.critical);
    }

    [TestMethod]
    public void GanttTask_WhenInitialize_ShouldHandleSlackValues()
    {
        var task = new GanttTask { slack = 3.5 };
        Assert.AreEqual(3.5, task.slack, 0.001);
    }

    [TestMethod]
    public void GanttTask_WhenInitializingProgress_ShouldBeADoubleBetween0And1()
    {
        var task = new GanttTask { progress = 0.6 };
        Assert.IsTrue(task.progress >= 0 && task.progress <= 1);
    }

    [TestMethod]
    public void GanttTask_WhenInitializingWithCorrectValues_ShouldSetAllPropertiesCorrectly()
    {
        var task = new GanttTask
        {
            id = 1,
            text = "Diseñar base de datos",
            start_date = "2025-05-12",
            end_date = "2025-05-17",
            duration = 5,
            progress = 0.75,
            critical = true,
            slack = 0
        };
        Assert.AreEqual(1, task.id);
        Assert.AreEqual("Diseñar base de datos", task.text);
        Assert.AreEqual("2025-05-12", task.start_date);
        Assert.AreEqual("2025-05-17", task.end_date);
        Assert.AreEqual(5, task.duration);
        Assert.AreEqual(0.75, task.progress, 0.001);
        Assert.IsTrue(task.critical);
        Assert.AreEqual(0, task.slack);
    }
}