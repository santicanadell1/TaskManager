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
        public void g()
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
        [TestMethod]
        public void CpmResultDTO_WhenProjectDurationIsSet_ThenProjectDurationIsAssigned()
        {
            int projectDuration = 10;
            CpmResultDTO resultDTO = new CpmResultDTO();
            
            resultDTO.ProjectDuration = projectDuration;
            
            Assert.AreEqual(projectDuration, resultDTO.ProjectDuration);
        }
        
        [TestMethod]
        public void CpmResultDTO_WhenCriticalPathIdsAreSet_ThenCriticalPathIdsAreAssigned()
        {
            List<int?> criticalPathIds = new List<int?> { 1, 3, 5 };
            CpmResultDTO resultDTO = new CpmResultDTO();
            
            resultDTO.CriticalPathIds = criticalPathIds;
            
            Assert.AreEqual(criticalPathIds, resultDTO.CriticalPathIds);
            Assert.AreEqual(3, resultDTO.CriticalPathIds.Count);
            Assert.AreEqual(1, resultDTO.CriticalPathIds[0]);
            Assert.AreEqual(3, resultDTO.CriticalPathIds[1]);
            Assert.AreEqual(5, resultDTO.CriticalPathIds[2]);
        }
        
        [TestMethod]
        public void CpmResultDTO_WhenCriticalTaskIdsAreSet_ThenCriticalTaskIdsAreAssigned()
        {
            List<int?> criticalTaskIds = new List<int?> { 1, 2, 3, 4 };
            CpmResultDTO resultDTO = new CpmResultDTO();
            
            resultDTO.CriticalTaskIds = criticalTaskIds;
            
            Assert.AreEqual(criticalTaskIds, resultDTO.CriticalTaskIds);
            Assert.AreEqual(4, resultDTO.CriticalTaskIds);
        }
        
    }
}