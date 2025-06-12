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

}