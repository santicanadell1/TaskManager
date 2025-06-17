using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service.Models;

namespace Service.Test.ModelsTests
{
    [TestClass]
    public class TaskExportDTOTest
    {
        [TestMethod]
        public void TaskExportDTO_ShouldSetTaskProperty()
        {
            TaskExportDTO taskExportDTO = new TaskExportDTO();
            taskExportDTO.Task = "Test Task";

            Assert.AreEqual("Test Task", taskExportDTO.Task);
        }

        [TestMethod]
        public void TaskExportDTO_ShouldSetStartDateProperty()
        {
            TaskExportDTO taskExportDTO = new TaskExportDTO();
            taskExportDTO.StartDate = "2024-01-01";

            Assert.AreEqual("2024-01-01", taskExportDTO.StartDate);
        }

        [TestMethod]
        public void TaskExportDTO_ShouldSetDurationProperty()
        {
            TaskExportDTO taskExportDTO = new TaskExportDTO();
            int duration = 5;
            taskExportDTO.Duration = duration;

            Assert.AreEqual(5, taskExportDTO.Duration);
        }

        [TestMethod]
        public void TaskExportDTO_ShouldSetIsCriticalProperty()
        {
            TaskExportDTO taskExportDTO = new TaskExportDTO();
            string isCritical = "Yes";
            taskExportDTO.IsCritical = isCritical;

            Assert.AreEqual("Yes", taskExportDTO.IsCritical);
        }

        [TestMethod]
        public void TaskExportDTO_ShouldSetResourcesProperty()
        {
            TaskExportDTO taskExportDTO = new TaskExportDTO();
            List<string> resources = new List<string> { "Resource1", "Resource2" };
            taskExportDTO.Resources = resources;

            Assert.AreEqual(2, taskExportDTO.Resources.Count);
            Assert.AreEqual("Resource1", taskExportDTO.Resources[0]);
        }
    }
}