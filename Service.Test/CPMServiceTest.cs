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
        private Task _taskC;
        private Task _taskD;

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
                new List<Task>(),
                new List<Task>(),
                new List<Resource>()
            );
            _taskB.Id = 2;

            _taskC = new Task(
                "Tarea C",
                "Descripción de Tarea C",
                new DateTime(2025, 1, 1),
                2,
                new List<Task> { _taskA },
                new List<Task>(),
                new List<Resource>()
            );
            _taskC.Id = 3;

            _taskD = new Task(
                "Tarea D",
                "Descripción de Tarea D",
                new DateTime(2025, 1, 1),
                3,
                new List<Task> { _taskB, _taskC },
                new List<Task>(),
                new List<Resource>()
            );
            _taskD.Id = 4;
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
            var tasks = new List<Task> { _taskA, _taskB, _taskC, _taskD };
            var result = _cpmService.CalculateCriticalPath(tasks);

            var taskA = result.AllTasks.First(t => t.Id == 1);
            var taskC = result.AllTasks.First(t => t.Id == 3);
            
            Assert.AreEqual(_taskA.ExpectedStartDate, taskA.StartDate);
            Assert.AreEqual(taskA.EndDate, taskC.StartDate);
            Assert.AreEqual(taskC.StartDate.AddDays(taskC.Duration), taskC.EndDate);
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldCalculateLateDates_AndSlack()
        {
            var tasks = new List<Task> { _taskA, _taskB };
            var taskBWithDependency = new Task(
                "Tarea B",
                "Descripción de Tarea B",
                new DateTime(2025, 1, 1),
                4,
                new List<Task> { _taskA },
                new List<Task>(),
                new List<Resource>()
            );
            taskBWithDependency.Id = 2;
            
            tasks = new List<Task> { _taskA, taskBWithDependency };
            var result = _cpmService.CalculateCriticalPath(tasks);

            var taskA = result.AllTasks.First(t => t.Id == 1);
            var taskB = result.AllTasks.First(t => t.Id == 2);
            
            Assert.IsNotNull(taskA.LatestStart);
            Assert.IsNotNull(taskA.LatestFinish);
            Assert.IsNotNull(taskB.LatestStart);
            Assert.IsNotNull(taskB.LatestFinish);
            
            Assert.AreEqual(0, taskA.Slack.TotalDays);
            Assert.AreEqual(0, taskB.Slack.TotalDays);
            
            Assert.IsTrue(taskA.IsCritical);
            Assert.IsTrue(taskB.IsCritical);
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldIdentifyCriticalPath_WithComplexDependencies()
        {
            var tasks = new List<Task> { _taskA, _taskB, _taskC, _taskD };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.IsTrue(result.CriticalPath.Count > 0);
            Assert.IsTrue(result.CriticalTasks.Count > 0);
            
            Assert.IsTrue(result.CriticalTasks.Any(t => t.Id == 4));
            
            var nonCriticalTasks = result.AllTasks.Where(t => !t.IsCritical).ToList();
            foreach (var task in nonCriticalTasks)
            {
                Assert.IsTrue(task.Slack.TotalDays > 0);
            }
            
            var criticalTasks = result.AllTasks.Where(t => t.IsCritical).ToList();
            foreach (var task in criticalTasks)
            {
                Assert.AreEqual(0, task.Slack.TotalDays, 0.0001);
            }
            
            Assert.IsTrue(result.CriticalPath.Any(t => t.Id == 2) || result.CriticalPath.Any(t => t.Id == 1));
            Assert.IsTrue(result.CriticalPath.Any(t => t.Id == 4));
        }
    }
}