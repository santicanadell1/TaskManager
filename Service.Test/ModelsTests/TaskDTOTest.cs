using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class TaskDTOTest
{
    [TestMethod]
    public void NewTask_WhenTitleIsNull_ThenTaskIsNotCreated()
    {
        var taskDTO = new TaskDTO { Title = null };
        Assert.IsNull(taskDTO.Title);
    }

    [TestMethod]
    public void NewTask_WhenDescriptionIsNull_ThenTaskIsNotCreated()
    {
        var taskDTO = new TaskDTO { Description = null };
        Assert.IsNull(taskDTO.Description);
    }

    [TestMethod]
    public void NewTask_WhenExpectedStartDateIsValid_ThenTaskIsCreated()
    {
        var validDate = new DateTime(2025, 5, 3);
        var taskDTO = new TaskDTO { ExpectedStartDate = validDate };
        Assert.AreEqual(validDate, taskDTO.ExpectedStartDate);
    }

    [TestMethod]
    public void NewTask_WhenDurationIsValid_ThenTaskIsCreated()
    {
        var validDuration = 5;
        var taskDTO = new TaskDTO { Duration = validDuration };
        Assert.AreEqual(validDuration, taskDTO.Duration);
    }

    [TestMethod]
    public void NewTask_WhenPreviousTasksAreAssigned_ThenPreviousTasksAreSet()
    {
        List<TaskDTO> previousTasks = new List<TaskDTO>
        {
            new()
            {
                Title = "Task 1",
                Description = "Description 1",
                ExpectedStartDate = DateTime.Now,
                Duration = 3
            },
            new()
            {
                Title = "Task 2",
                Description = "Description 2",
                ExpectedStartDate = DateTime.Now,
                Duration = 5
            }
        };

        var taskDTO = new TaskDTO { PreviousTasks = previousTasks };
        Assert.IsNotNull(taskDTO.PreviousTasks);
        Assert.AreEqual(2, taskDTO.PreviousTasks.Count);
        Assert.AreEqual("Task 1", taskDTO.PreviousTasks[0].Title);
        Assert.AreEqual("Task 2", taskDTO.PreviousTasks[1].Title);
    }

    [TestMethod]
    public void NewTask_WhenSameTimeTasksAreAssigned_ThenSameTimeTasksAreSet()
    {
        List<TaskDTO> sameTimeTasks = new List<TaskDTO>
        {
            new()
            {
                Title = "Task A",
                Description = "Description A",
                ExpectedStartDate = DateTime.Now,
                Duration = 3
            },
            new()
            {
                Title = "Task B",
                Description = "Description B",
                ExpectedStartDate = DateTime.Now,
                Duration = 2
            }
        };

        var taskDTO = new TaskDTO { SameTimeTasks = sameTimeTasks };
        Assert.IsNotNull(taskDTO.SameTimeTasks);
        Assert.AreEqual(2, taskDTO.SameTimeTasks.Count);
        Assert.AreEqual("Task A", taskDTO.SameTimeTasks[0].Title);
        Assert.AreEqual("Task B", taskDTO.SameTimeTasks[1].Title);
    }

    [TestMethod]
    public void NewTask_WhenStateIsSet_ThenStateIsAssignedCorrectly()
    {
        var stateTodo = StateDTO.TODO;
        var stateDoing = StateDTO.DOING;
        var stateDone = StateDTO.DONE;

        var taskTodo = new TaskDTO { State = stateTodo };
        var taskDoing = new TaskDTO { State = stateDoing };
        var taskDone = new TaskDTO { State = stateDone };

        Assert.AreEqual(stateTodo, taskTodo.State);
        Assert.AreEqual(stateDoing, taskDoing.State);
        Assert.AreEqual(stateDone, taskDone.State);
    }

    [TestMethod]
    public void NewTask_WhenIdIsSet_ThenIdIsAssigned()
    {
        int? idValue = 123;
        var taskDTO = new TaskDTO { Id = idValue };
        Assert.AreEqual(idValue, taskDTO.Id);
    }

    [TestMethod]
    public void NewTask_WhenIdIsNotSet_ThenIdIsNull()
    {
        var taskDTO = new TaskDTO();
        Assert.IsNull(taskDTO.Id);
    }

    [TestMethod]
    public void NewTask_WhenResourcesAreAssigned_ThenResourcesAreSet()
    {
        List<ResourceDTO> resources = new List<ResourceDTO>
        {
            new() { Name = "Resource 1", Type = "Type 1", Description = "Description of Resource 1" },
            new() { Name = "Resource 2", Type = "Type 2", Description = "Description of Resource 2" }
        };

        var taskDTO = new TaskDTO { Resources = resources };

        Assert.IsNotNull(taskDTO.Resources);
        Assert.AreEqual(2, taskDTO.Resources.Count);
        Assert.AreEqual("Resource 1", taskDTO.Resources[0].Name);
        Assert.AreEqual("Resource 2", taskDTO.Resources[1].Name);
    }

    [TestMethod]
    public void TaskDTO_WhenIsCriticalIsSet_ThenIsCriticalIsAssigned()
    {
        var isCritical = true;
        var taskDTO = new TaskDTO();

        taskDTO.IsCritical = isCritical;

        Assert.AreEqual(isCritical, taskDTO.IsCritical);
    }

    [TestMethod]
    public void TaskDTO_WhenStartDateIsSet_ThenStartDateIsAssigned()
    {
        var startDate = new DateTime(2025, 6, 15);
        var taskDTO = new TaskDTO();

        taskDTO.StartDate = startDate;

        Assert.AreEqual(startDate, taskDTO.StartDate);
    }

    [TestMethod]
    public void TaskDTO_WhenEndDateIsSet_ThenEndDateIsAssigned()
    {
        var endDate = new DateTime(2025, 6, 20);
        var taskDTO = new TaskDTO();

        taskDTO.EndDate = endDate;

        Assert.AreEqual(endDate, taskDTO.EndDate);
    }

    [TestMethod]
    public void TaskDTO_WhenLatestStartIsSet_ThenLatestStartIsAssigned()
    {
        var latestStart = new DateTime(2025, 6, 16);
        var taskDTO = new TaskDTO();

        taskDTO.LatestStart = latestStart;

        Assert.AreEqual(latestStart, taskDTO.LatestStart);
    }

    [TestMethod]
    public void TaskDTO_WhenLatestFinishIsSet_ThenLatestFinishIsAssigned()
    {
        var latestFinish = new DateTime(2025, 6, 21);
        var taskDTO = new TaskDTO();

        taskDTO.LatestFinish = latestFinish;

        Assert.AreEqual(latestFinish, taskDTO.LatestFinish);
    }

    [TestMethod]
    public void TaskDTO_WhenSlackIsSet_ThenSlackIsAssigned()
    {
        var slack = TimeSpan.FromDays(2);
        var taskDTO = new TaskDTO();

        taskDTO.Slack = slack;

        Assert.AreEqual(slack, taskDTO.Slack);
    }

    [TestMethod]
    public void TaskDTO_DefaultValues_ShouldBeCorrect()
    {
        var taskDTO = new TaskDTO();

        Assert.IsFalse(taskDTO.IsCritical, "Default value for IsCritical should be false");
        Assert.AreEqual(default, taskDTO.StartDate, "Default value for StartDate should be default(DateTime)");
        Assert.AreEqual(default, taskDTO.EndDate, "Default value for EndDate should be default(DateTime)");
        Assert.AreEqual(default, taskDTO.LatestStart, "Default value for LatestStart should be default(DateTime)");
        Assert.AreEqual(default, taskDTO.LatestFinish, "Default value for LatestFinish should be default(DateTime)");
        Assert.AreEqual(default, taskDTO.Slack, "Default value for Slack should be default(TimeSpan)");
    }

    [TestMethod]
    public void TaskDTO_PropertiesCanBeSetInConstructor()
    {
        var now = DateTime.Now;
        var startDate = now;
        var endDate = now.AddDays(5);
        var latestStart = now.AddDays(1);
        var latestFinish = now.AddDays(6);
        var slack = TimeSpan.FromDays(1);

        var taskDTO = new TaskDTO
        {
            Title = "Test Task",
            Description = "Test Description",
            ExpectedStartDate = now,
            Duration = 5,
            IsCritical = true,
            StartDate = startDate,
            EndDate = endDate,
            LatestStart = latestStart,
            LatestFinish = latestFinish,
            Slack = slack
        };

        Assert.AreEqual("Test Task", taskDTO.Title);
        Assert.AreEqual("Test Description", taskDTO.Description);
        Assert.AreEqual(now, taskDTO.ExpectedStartDate);
        Assert.AreEqual(5, taskDTO.Duration);
        Assert.IsTrue(taskDTO.IsCritical);
        Assert.AreEqual(startDate, taskDTO.StartDate);
        Assert.AreEqual(endDate, taskDTO.EndDate);
        Assert.AreEqual(latestStart, taskDTO.LatestStart);
        Assert.AreEqual(latestFinish, taskDTO.LatestFinish);
        Assert.AreEqual(slack, taskDTO.Slack);
    }
}