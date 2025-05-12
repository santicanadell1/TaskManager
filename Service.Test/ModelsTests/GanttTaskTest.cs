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
    }
