using System.ComponentModel.DataAnnotations;
using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class TaskExportDTOTest
{
    [TestMethod]
    public void TaskExportDTO_ShouldSetTaskProperty()
    {
        var taskExportDTO = new TaskExportDTO();
        taskExportDTO.Task = "Test Task";
        
        Assert.AreEqual("Test Task", taskExportDTO.Task);
    }
    
    [TestMethod]
    public void TaskExportDTO_ShouldSetStartDateProperty()
    {
        var taskExportDTO = new TaskExportDTO();
        taskExportDTO.StartDate = "2024-01-01";
        
        Assert.AreEqual("2024-01-01", taskExportDTO.StartDate);
    }

    [TestMethod]
    public void TaskExportDTO_ShouldSetDurationProperty()
    {
        var taskExportDTO = new TaskExportDTO();
        taskExportDTO.Duration = 5;
        
        Assert.AreEqual(5, taskExportDTO.Duration);
    }


    
}