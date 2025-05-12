using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service.Models;

namespace Service.Test.ModelsTests;
    [TestClass]
    public class GanttTaskTests
    {
        [TestMethod]
        public void GanttTask_ShouldAllowNonCriticalTasks()
        {
            // Arrange
            var task = new GanttTask { critical = false };

            // Assert
            Assert.IsFalse(task.critical);
        }
        [TestMethod]
        public void GanttTask_ShouldHandleSlackValues()
        {
            // Arrange
            var task = new GanttTask { slack = 3.5 };

            // Assert
            Assert.AreEqual(3.5, task.slack, 0.001);
        }
    }
