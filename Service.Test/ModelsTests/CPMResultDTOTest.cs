using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class CpmResultDTOTest
{
    [TestMethod]
    public void CpmResultDTO_PropertiesAreAssignedCorrectly()
    {
        var projectDuration = 15;
        var criticalPathIds = new List<int?> { 1, 2, 3 };
        var criticalTaskIds = new List<int?> { 1, 2, 3, 4 };
        var earliestStartDate = new DateTime(2025, 1, 1);
        var latestFinishDate = new DateTime(2025, 1, 16);

        var resultDTO = new CpmResultDTO
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
        var projectDuration = 10;
        var resultDTO = new CpmResultDTO();

        resultDTO.ProjectDuration = projectDuration;

        Assert.AreEqual(projectDuration, resultDTO.ProjectDuration);
    }

    [TestMethod]
    public void CpmResultDTO_WhenCriticalPathIdsAreSet_ThenCriticalPathIdsAreAssigned()
    {
        var criticalPathIds = new List<int?> { 1, 3, 5 };
        var resultDTO = new CpmResultDTO();

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
        var criticalTaskIds = new List<int?> { 1, 2, 3, 4 };
        var resultDTO = new CpmResultDTO();

        resultDTO.CriticalTaskIds = criticalTaskIds;

        Assert.AreEqual(criticalTaskIds, resultDTO.CriticalTaskIds);
        Assert.AreEqual(4, resultDTO.CriticalTaskIds.Count);
    }

    [TestMethod]
    public void CpmResultDTO_WhenEarliestStartDateIsSet_ThenEarliestStartDateIsAssigned()
    {
        var earliestStartDate = new DateTime(2025, 3, 15);
        var resultDTO = new CpmResultDTO();

        resultDTO.EarliestStartDate = earliestStartDate;

        Assert.AreEqual(earliestStartDate, resultDTO.EarliestStartDate);
    }

    [TestMethod]
    public void CpmResultDTO_WhenLatestFinishDateIsSet_ThenLatestFinishDateIsAssigned()
    {
        var latestFinishDate = new DateTime(2025, 4, 30);
        var resultDTO = new CpmResultDTO();

        resultDTO.LatestFinishDate = latestFinishDate;

        Assert.AreEqual(latestFinishDate, resultDTO.LatestFinishDate);
    }

    [TestMethod]
    public void CpmResultDTO_DefaultValues_ShouldBeCorrect()
    {
        var resultDTO = new CpmResultDTO();

        Assert.AreEqual(0, resultDTO.ProjectDuration);
        Assert.IsNull(resultDTO.CriticalPathIds);
        Assert.IsNull(resultDTO.CriticalTaskIds);
        Assert.AreEqual(default, resultDTO.EarliestStartDate);
        Assert.AreEqual(default, resultDTO.LatestFinishDate);
    }
}