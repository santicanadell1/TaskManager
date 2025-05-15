using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class GanttDataTests
{
    [TestMethod]
    public void GanttData_WhenInitializingWithEmptyLists_ShouldInitializeCorrectly()
    {
        var ganttData = new GanttData
        {
            data = new List<GanttTask>(),
            links = new List<GanttLink>(),
            criticalPathIds = new List<int>()
        };
        Assert.IsNotNull(ganttData.data);
        Assert.IsNotNull(ganttData.links);
        Assert.IsNotNull(ganttData.criticalPathIds);
        Assert.AreEqual(0, ganttData.data.Count);
        Assert.AreEqual(0, ganttData.links.Count);
        Assert.AreEqual(0, ganttData.criticalPathIds.Count);
    }

    [TestMethod]
    public void GanttData_WhenInitializeWithCorrectData_ShouldHoldProvidedData()
    {
        var task = new GanttTask { id = 1, text = "Tarea 1", start_date = "2025-05-10", duration = 5, progress = 0.5 };
        var link = new GanttLink { id = 1, source = 1, target = 2, type = "0", critical = true };
        var criticalIds = new List<int> { 1, 2, 3 };
        var ganttData = new GanttData
        {
            data = new List<GanttTask> { task },
            links = new List<GanttLink> { link },
            criticalPathIds = criticalIds
        };
        Assert.AreEqual(1, ganttData.data.Count);
        Assert.AreEqual("Tarea 1", ganttData.data[0].text);
        Assert.AreEqual(1, ganttData.links.Count);
        Assert.IsTrue(ganttData.links[0].critical);
        Assert.AreEqual(3, ganttData.criticalPathIds.Count);
        CollectionAssert.AreEqual(criticalIds, ganttData.criticalPathIds);
    }
}