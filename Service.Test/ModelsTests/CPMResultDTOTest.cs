using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service.Models;
using System;
using System.Collections.Generic;

namespace Service.Test.ModelsTests
{
    [TestClass]
    public class CpmResultDTOTest
    {
        [TestMethod]
        public void CpmResultDTO_PropertiesAreAssignedCorrectly()
        {
            int projectDuration = 15;
            List<int?> criticalPathIds = new List<int?> { 1, 2, 3 };
            List<int?> criticalTaskIds = new List<int?> { 1, 2, 3, 4 };
            DateTime earliestStartDate = new DateTime(2025, 1, 1);
            DateTime latestFinishDate = new DateTime(2025, 1, 16);

            CpmResultDTO resultDTO = new CpmResultDTO
            {
                ProjectDuration = projectDuration,
                CriticalPathIds = criticalPathIds,
                CriticalTaskIds = criticalTaskIds,
                EarliestStartDate = earliestStartDate,
                LatestFinishDate = latestFinishDate
            };

            Assert.AreEqual(projectDuration, resultDTO.ProjectDuration);
            Assert.AreEqual(criticalPathIds, resultDTO.CriticalPathIds);
            Assert.AreEqual(criticalTaskIds, resultDTO.CriticalTaskIds);
            Assert.AreEqual(earliestStartDate, resultDTO.EarliestStartDate);
            Assert.AreEqual(latestFinishDate, resultDTO.LatestFinishDate);
        }
    }
}