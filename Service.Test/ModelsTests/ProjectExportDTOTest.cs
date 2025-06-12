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
    

}