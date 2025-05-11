using Domain;
using Domain.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = Domain.Task;

namespace Service.Test
{
    [TestClass]
    public class CpmServiceTest
    {
        private CpmService _cpmService;
        private Task _taskA;
        private Task _taskB;

        [TestInitialize]
        public void Setup()
        {
            _cpmService = new CpmService();
            
            _taskA = new Task(
                "Tarea A",
                "Descripción de Tarea A",
                new DateTime(2025, 1, 1),
                3,
                new List<Task>(),
                new List<Task>(),
                new List<Resource>()
            );
            _taskA.Id = 1;

            _taskB = new Task(
                "Tarea B",
                "Descripción de Tarea B",
                new DateTime(2025, 1, 1),
                4,
                new List<Task> { _taskA },
                new List<Task>(),
                new List<Resource>()
            );
            _taskB.Id = 2;
        }

        [TestMethod]
        [ExpectedException(typeof(NullTaskListException))]
        public void CalculateCriticalPath_ShouldThrowException_WhenTasksListIsNull()
        {
            _cpmService.CalculateCriticalPath(null);
        }

        [TestMethod]
        [ExpectedException(typeof(EmptyTaskListException))]
        public void CalculateCriticalPath_ShouldThrowException_WhenTasksListIsEmpty()
        {
            _cpmService.CalculateCriticalPath(new List<Task>());
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldReturnValidResult_WhenSingleTaskExists()
        {
            var singleTask = new List<Task> { _taskA };
            var result = _cpmService.CalculateCriticalPath(singleTask);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AllTasks);
            Assert.IsNotNull(result.CriticalPath);
            Assert.IsNotNull(result.CriticalTasks);
            
            Assert.AreEqual(1, result.AllTasks.Count);
            Assert.AreEqual(1, result.CriticalPath.Count);
            Assert.AreEqual(1, result.CriticalTasks.Count);
            Assert.AreEqual(_taskA.Duration, result.ProjectDuration);
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldCalculateEarlyDates_ForTaskWithoutPredecessors()
        {
            var singleTask = new List<Task> { _taskA };
            var result = _cpmService.CalculateCriticalPath(singleTask);

            var processedTask = result.AllTasks.First();
            
            Assert.AreEqual(_taskA.ExpectedStartDate, processedTask.StartDate);
            Assert.AreEqual(_taskA.ExpectedStartDate.AddDays(_taskA.Duration), processedTask.EndDate);
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldCalculateEarlyDates_ForTaskWithPredecessors()
        {
            var tasks = new List<Task> { _taskA, _taskB };
            var result = _cpmService.CalculateCriticalPath(tasks);

            var taskA = result.AllTasks.First(t => t.Id == 1);
            var taskB = result.AllTasks.First(t => t.Id == 2);
            
            Assert.AreEqual(_taskA.ExpectedStartDate, taskA.StartDate);
            Assert.AreEqual(taskA.EndDate, taskB.StartDate);
            Assert.AreEqual(taskB.StartDate.AddDays(taskB.Duration), taskB.EndDate);
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldCalculateLateDates_AndSlack()
        {
            var tasks = new List<Task> { _taskA, _taskB };
            var result = _cpmService.CalculateCriticalPath(tasks);

            var taskA = result.AllTasks.First(t => t.Id == 1);
            var taskB = result.AllTasks.First(t => t.Id == 2);
            
            Assert.IsNotNull(taskA.LatestStart);
            Assert.IsNotNull(taskA.LatestFinish);
            Assert.IsNotNull(taskB.LatestStart);
            Assert.IsNotNull(taskB.LatestFinish);
            
            Assert.AreEqual(0, taskA.Slack.TotalDays);
            Assert.AreEqual(0, taskB.Slack.TotalDays);
            
            Assert.IsTrue(taskA);
            Assert.IsTrue(taskB.IsCritical);
        }
    }
}