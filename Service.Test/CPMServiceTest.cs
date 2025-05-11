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

        [TestMethod]
        [ExpectedException(typeof(CircularDependencyException))]
        public void CalculateCriticalPath_ShouldThrowException_WhenCircularDependencyExists()
        {
            _taskA.PreviousTasks.Add(_taskB);
            _taskB.PreviousTasks.Add(_taskA);

            var circularTasks = new List<Task> { _taskA, _taskB };
            _cpmService.CalculateCriticalPath(circularTasks);
        }

        [TestMethod]
        public void CalculateEarlyStart_ShouldReturnExpectedStartDate_WhenNoPreviousTasks()
        {
            var earlyStart = _cpmService.CalculateEarlyStart(_taskA);
            Assert.AreEqual(_taskA.ExpectedStartDate, earlyStart);
        }

        
        [TestMethod]
        public void CalculateEarlyStart_ShouldReturnLatestPreviousEnd_WhenHasPreviousTasks()
        {
            _taskA.EndDate = new DateTime(2025, 1, 4);
            var earlyStart = _cpmService.CalculateEarlyStart(_taskC);
            Assert.AreEqual(_taskA.EndDate, earlyStart);
        }

        [TestMethod]
        public void CalculateEarlyFinish_ShouldReturnStartPlusDuration()
        {
            _taskA.StartDate = new DateTime(2025, 1, 1);
            var earlyFinish = _cpmService.CalculateEarlyFinish(_taskA);
            Assert.AreEqual(_taskA.StartDate.AddDays(_taskA.Duration), earlyFinish);
        }

        [TestMethod]
        public void CalculateLateStart_ShouldReturnStartDate_WhenNoSuccessors()
        {
            var testTask = new Task(
                "Test Task",
                "Description",
                new DateTime(2025, 1, 1),
                3,
                new List<Task>(),
                new List<Task>(),
                new List<Resource>()
            );

            testTask.StartDate = new DateTime(2025, 1, 1);
            testTask.EndDate = testTask.StartDate.AddDays(testTask.Duration);

            var taskList = new List<Task> { testTask };

            var lateStart = _cpmService.CalculateLateStart(testTask, taskList);

            Assert.AreEqual(testTask.StartDate, lateStart);
        }

        [TestMethod]
        public void IsCritical_ShouldReturnTrue_WhenSlackIsZero()
        {
            _taskA.Slack = TimeSpan.Zero;
            Assert.IsTrue(_cpmService.IsCritical(_taskA));
        }

        [TestMethod]
        public void IsCritical_ShouldReturnFalse_WhenSlackIsNotZero()
        {
            _taskA.Slack = TimeSpan.FromDays(2);
            Assert.IsFalse(_cpmService.IsCritical(_taskA));
        }
        
        [TestMethod]
        public void CalculateLateFinish_ShouldReturnMinSuccessorLatestStart_WhenHasSuccessors()
        {
            _taskC.LatestStart = new DateTime(2025, 1, 6);
            _taskD.LatestStart = new DateTime(2025, 1, 4);
            
            _taskC.PreviousTasks = new List<Task> { _taskA };
            _taskD.PreviousTasks = new List<Task> { _taskA };
            
            var tasks = new List<Task> { _taskA, _taskC, _taskD };
            var lateFinish = _cpmService.CalculateLateFinish(_taskA, tasks);
            
            Assert.AreEqual(_taskD.LatestStart, lateFinish);
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldHandleMultipleParallelPaths()
        {
            var taskE = new Task(
                "Tarea E",
                "Descripción de Tarea E",
                new DateTime(2025, 1, 1),
                6,
                new List<Task>(),
                new List<Task>(),
                new List<Resource>()
            );
            taskE.Id = 5;

            var taskF = new Task(
                "Tarea F",
                "Descripción de Tarea F",
                new DateTime(2025, 1, 1),
                2,
                new List<Task> { taskE },
                new List<Task>(),
                new List<Resource>()
            );
            taskF.Id = 6;

            var taskG = new Task(
                "Tarea G",
                "Descripción de Tarea G",
                new DateTime(2025, 1, 1),
                1,
                new List<Task> { _taskD, taskF },
                new List<Task>(),
                new List<Resource>()
            );
            taskG.Id = 7;

            var tasks = new List<Task> { _taskA, _taskB, _taskC, _taskD, taskE, taskF, taskG };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.IsTrue(result.CriticalPath.Count > 0);
            Assert.IsTrue(result.CriticalTasks.Count > 0);

            Assert.IsTrue(result.CriticalTasks.Any(t => t.Id == 7));

            var criticalIds = result.CriticalTasks.Select(t => t.Id).ToList();
            Assert.IsTrue(criticalIds.Contains(5) && criticalIds.Contains(6));

            Assert.IsTrue(result.ProjectDuration >= 9);

            for (int i = 0; i < result.CriticalPath.Count - 1; i++)
            {
                var currentTask = result.CriticalPath[i];
                var nextTask = result.CriticalPath[i + 1];

                Assert.IsTrue(nextTask.PreviousTasks.Contains(currentTask) ||
                              nextTask.PreviousTasks.Any(p => result.CriticalPath.Contains(p)),
                    $"El camino crítico no está ordenado correctamente: {currentTask.Title} -> {nextTask.Title}");
            }
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldHandleParallelTasksWithSameStartEnd()
        {
            var parallelTask1 = new Task(
                "Paralela 1",
                "Descripción",
                new DateTime(2025, 1, 1),
                5,
                new List<Task>(),
                new List<Task>(),
                new List<Resource>()
            );
            parallelTask1.Id = 10;

            var parallelTask2 = new Task(
                "Paralela 2",
                "Descripción",
                new DateTime(2025, 1, 1),
                5,
                new List<Task>(),
                new List<Task>(),
                new List<Resource>()
            );
            parallelTask2.Id = 11;

            var finalTask = new Task(
                "Final",
                "Descripción",
                new DateTime(2025, 1, 1),
                2,
                new List<Task> { parallelTask1, parallelTask2 },
                new List<Task>(),
                new List<Resource>()
            );
            finalTask.Id = 12;

            var tasks = new List<Task> { parallelTask1, parallelTask2, finalTask };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.AreEqual(3, result.CriticalTasks.Count);
            Assert.AreEqual(7, result.ProjectDuration);

            Assert.IsTrue(result.CriticalTasks.Any(t => t.Id == 10));
            Assert.IsTrue(result.CriticalTasks.Any(t => t.Id == 11));
            Assert.IsTrue(result.CriticalTasks.Any(t => t.Id == 12));
        }

        [TestMethod]
        public void CalculateLateFinish_ShouldReturnEndDate_WhenNoSuccessors()
        {
            var testTask = new Task(
                "Test Task",
                "Description",
                new DateTime(2025, 1, 1),
                3,
                new List<Task>(),
                new List<Task>(),
                new List<Resource>()
            );

            testTask.StartDate = new DateTime(2025, 1, 1);
            testTask.EndDate = testTask.StartDate.AddDays(testTask.Duration);

            var taskList = new List<Task> { testTask };

            var lateFinish = _cpmService.CalculateLateFinish(testTask, taskList);

            Assert.AreEqual(testTask.EndDate, lateFinish);
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldHandleCompleteProjectNetwork()
        {
            var task1 = new Task("Task 1", "Description", new DateTime(2025, 1, 1), 5,
                new List<Task>(), new List<Task>(), new List<Resource>()) { Id = 1 };

            var task2 = new Task("Task 2", "Description", new DateTime(2025, 1, 1), 3,
                new List<Task> { task1 }, new List<Task>(), new List<Resource>()) { Id = 2 };

            var task3 = new Task("Task 3", "Description", new DateTime(2025, 1, 1), 4,
                new List<Task> { task1 }, new List<Task>(), new List<Resource>()) { Id = 3 };

            var task4 = new Task("Task 4", "Description", new DateTime(2025, 1, 1), 2,
                new List<Task> { task2, task3 }, new List<Task>(), new List<Resource>()) { Id = 4 };

            var task5 = new Task("Task 5", "Description", new DateTime(2025, 1, 1), 3,
                new List<Task> { task3 }, new List<Task>(), new List<Resource>()) { Id = 5 };

            var task6 = new Task("Task 6", "Description", new DateTime(2025, 1, 1), 1,
                new List<Task> { task4, task5 }, new List<Task>(), new List<Resource>()) { Id = 6 };

            var tasks = new List<Task> { task1, task2, task3, task4, task5, task6 };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.AreEqual(6, result.AllTasks.Count);
            Assert.IsTrue(result.CriticalPath.Count > 0);
            Assert.IsTrue(result.CriticalPath.First().PreviousTasks.Count == 0);
            Assert.IsTrue(!IsSuccessorOfAny(result.CriticalPath.Last(), tasks));

            for (int i = 0; i < result.CriticalPath.Count - 1; i++)
            {
                var current = result.CriticalPath[i];
                var next = result.CriticalPath[i + 1];
                Assert.IsTrue(next.PreviousTasks.Contains(current));
            }

            Assert.AreEqual(13, result.ProjectDuration);

            foreach (var criticalTask in result.CriticalTasks)
            {
                Assert.AreEqual(0, criticalTask.Slack.TotalDays, 0.0001);
            }
        }

        private bool IsSuccessorOfAny(Task task, List<Task> allTasks)
        {
            return allTasks.Any(t => t.PreviousTasks.Contains(task));
        }
    }

}