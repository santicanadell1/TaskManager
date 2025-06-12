using System.ComponentModel.DataAnnotations;
using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class ProjectExportDTOTest
{
    [TestMethod]
    public void ProjectExportDTO_ShouldSetProjectProperty()
    {
        var projectExportDTO = new ProjectExportDTO
        {
            Project = "Test Project"
        };

        Assert.AreEqual("Test Project", projectExportDTO.Project);
    }
    
    [TestMethod]
    public void ProjectExportDTO_ShouldSetStartDateProperty()
    {
        var projectExportDTO = new ProjectExportDTO
        {
            StartDate = "2024-01-01"
        };

        Assert.AreEqual("2024-01-01", projectExportDTO.StartDate);
    }
    
    [TestMethod]
    public void ProjectExportDTO_ShouldAllowNullTasks()
    {
        var projectExportDTO = new ProjectExportDTO
        {
            Project = "Test Project",
            StartDate = "2024-01-01",
            Tasks = null
        };

        Assert.IsNull(projectExportDTO.Tasks);
    }
    
    [TestMethod]
    public void ProjectExportDTO_ShouldCreateValidObject_WhenAllPropertiesAreSet()
    {
        var task = new TaskExportDTO
        {
            Task = "Test Task",
            StartDate = "2024-01-01",
            Duration = 5,
            IsCritical = "Yes",
            Resources = new List<string> { "Resource1" }
        };

        var projectExportDTO = new ProjectExportDTO
        {
            Project = "Test Project",
            StartDate = "2024-01-01",
            Tasks = new List<TaskExportDTO> { task }
        };

        Assert.AreEqual("Test Project", projectExportDTO.Project);
        Assert.AreEqual("2024-01-01", projectExportDTO.StartDate);
        Assert.AreEqual(1, projectExportDTO.Tasks.Count);
    }
    

    [TestMethod]
    public void ProjectExportDTO_ShouldHandleEmptyTasksList()
    {
        var projectExportDTO = new ProjectExportDTO
        {
            
            Project = "Empty Project",
            StartDate = "2024-01-01",
            Tasks = new List<TaskExportDTO>()
        };

        Assert.IsNotNull(projectExportDTO.Tasks);
        Assert.AreEqual(0, projectExportDTO.Tasks.Count);
    }

    

}