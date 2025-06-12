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

    
}