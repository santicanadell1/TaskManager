using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
    public class GanttDataTests
    {
        [TestMethod]
        public void GanttData_ShouldInitializeWithEmptyLists()
        {
            // Arrange
            var ganttData = new GanttData
            {
                data = new List<GanttTask>(),
                links = new List<GanttLink>(),
                criticalPathIds = new List<int>()
            };

            // Assert
            Assert.IsNotNull(ganttData.data);
            Assert.IsNotNull(ganttData.links);
            Assert.IsNotNull(ganttData.criticalPathIds);
            Assert.AreEqual(0, ganttData.data.Count);
            Assert.AreEqual(0, ganttData.links.Count);
            Assert.AreEqual(0, ganttData.criticalPathIds.Count);
        }

        [TestMethod]
        public void GanttData_ShouldHoldProvidedData()
        {
            throw new NotImplementedException();
        }
        
    }