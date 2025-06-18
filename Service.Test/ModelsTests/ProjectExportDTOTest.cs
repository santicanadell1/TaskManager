using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    [TestMethod]
    public void ProjectExportDTO_ShouldHandleMultipleTasks()
    {
        var task1 = new TaskExportDTO
        {
            Task = "Task 1",
            StartDate = "2024-01-01",
            Duration = 3,
            IsCritical = "No",
            Resources = new List<string> { "Resource1" }
        };
        var task2 = new TaskExportDTO
        {
            Task = "Task 2",
            StartDate = "2024-01-04",
            Duration = 7,
            IsCritical = "Yes",
            Resources = new List<string> { "Resource2", "Resource3" }
        };

        var projectExportDTO = new ProjectExportDTO
        {
            Project = "Multi-Task Project",
            StartDate = "2024-01-01",
            Tasks = new List<TaskExportDTO> { task1, task2 }
        };

        Assert.AreEqual(2, projectExportDTO.Tasks.Count);
        Assert.AreEqual("Task 1", projectExportDTO.Tasks[0].Task);
        Assert.AreEqual("Task 2", projectExportDTO.Tasks[1].Task);
        Assert.AreEqual("Yes", projectExportDTO.Tasks[1].IsCritical);
    }
}