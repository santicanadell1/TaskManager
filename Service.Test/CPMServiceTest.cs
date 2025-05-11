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
        private DateTime _defaultStartDate = new DateTime(2025, 1, 1);

        [TestInitialize]
        public void Setup()
        {
            _cpmService = new CpmService();
            InitializeTasks();
        }

        private void InitializeTasks()
        {
            _taskA = CreateTask("Tarea A", "Descripción de Tarea A", 3, 1);
            _taskB = CreateTask("Tarea B", "Descripción de Tarea B", 4, 2);
            _taskC = CreateTask("Tarea C", "Descripción de Tarea C", 2, 3, new List<Task> { _taskA });
            _taskD = CreateTask("Tarea D", "Descripción de Tarea D", 3, 4, new List<Task> { _taskB, _taskC });
        }

        private Task CreateTask(string title, string description, int duration, int id, List<Task> previousTasks = null)
        {
            var task = new Task(
                title,
                description,
                _defaultStartDate,
                duration,
                previousTasks ?? new List<Task>(),
                new List<Task>(),
                new List<Resource>()
            );
            task.Id = id;
            return task;
        }

        #region Exception Tests

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
        [ExpectedException(typeof(CircularDependencyException))]
        public void CalculateCriticalPath_ShouldThrowException_WhenCircularDependencyExists()
        {
            _taskA.PreviousTasks.Add(_taskB);
            _taskB.PreviousTasks.Add(_taskA);

            _cpmService.CalculateCriticalPath(new List<Task> { _taskA, _taskB });
        }

        #endregion

        #region Single Task Tests

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

        #endregion

        #region Early Dates Tests

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

        #endregion

        #region Late Dates and Slack Tests

        [TestMethod]
        public void CalculateCriticalPath_ShouldCalculateLateDates_AndSlack()
        {
            var taskBWithDependency = CreateTask("Tarea B", "Descripción de Tarea B", 4, 2, new List<Task> { _taskA });
            var tasks = new List<Task> { _taskA, taskBWithDependency };
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
        public void CalculateLateStart_ShouldReturnStartDate_WhenNoSuccessors()
        {
            var testTask = CreateTask("Test Task", "Description", 3, 5);
            testTask.StartDate = _defaultStartDate;
            testTask.EndDate = testTask.StartDate.AddDays(testTask.Duration);

            var taskList = new List<Task> { testTask };
            var lateStart = _cpmService.CalculateLateStart(testTask, taskList);

            Assert.AreEqual(testTask.StartDate, lateStart);
        }

        [TestMethod]
        public void CalculateLateFinish_ShouldReturnEndDate_WhenNoSuccessors()
        {
            var testTask = CreateTask("Test Task", "Description", 3, 5);
            testTask.StartDate = _defaultStartDate;
            testTask.EndDate = testTask.StartDate.AddDays(testTask.Duration);

            var taskList = new List<Task> { testTask };
            var lateFinish = _cpmService.CalculateLateFinish(testTask, taskList);

            Assert.AreEqual(testTask.EndDate, lateFinish);
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
        public void CalculateLateStart_ShouldHandleInitializedLatestStart()
        {
            var task = CreateTask("Test Task", "Description", 3, 5);
            task.LatestStart = new DateTime(2025, 1, 5);

            var result = _cpmService.CalculateLateStart(task, new List<Task> { task });

            Assert.AreEqual(new DateTime(2025, 1, 5), result);
        }

        [TestMethod]
        public void CalculateLateStart_ShouldUseExpectedStartDate_WhenOtherDatesNotSet()
        {
            var task = CreateTask("Test Task", "Description", 3, 5);
            var result = _cpmService.CalculateLateStart(task, new List<Task> { task });

            Assert.AreEqual(_defaultStartDate, result);
        }

        [TestMethod]
        public void CalculateLateStart_ShouldHandleMultipleSuccessors_WithDifferentLatestStarts()
        {
            var taskA = CreateTask("Task A", "Description", 2, 1);
            var taskB = CreateTask("Task B", "Description", 1, 2, new List<Task> { taskA });
            var taskC = CreateTask("Task C", "Description", 3, 3, new List<Task> { taskA });
            var taskD = CreateTask("Task D", "Description", 2, 4, new List<Task> { taskA });

            taskB.LatestStart = new DateTime(2025, 1, 8);
            taskC.LatestStart = new DateTime(2025, 1, 5);
            taskD.LatestStart = new DateTime(2025, 1, 7);

            var tasks = new List<Task> { taskA, taskB, taskC, taskD };
            var result = _cpmService.CalculateLateStart(taskA, tasks);

            Assert.AreEqual(new DateTime(2025, 1, 3), result);
        }

        [TestMethod]
        public void CalculateLateDates_HandlesNoSuccessors()
        {
            var task = CreateTask("Solo Task", "Description", 3, 5);
            var tasks = new List<Task> { task };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.AreEqual(task.EndDate, task.LatestFinish);
            Assert.AreEqual(task.LatestFinish.AddDays(-task.Duration), task.LatestStart);
        }

        #endregion

        #region Critical Path Tests

        [TestMethod]
        public void CalculateCriticalPath_ShouldIdentifyCriticalPath_WithComplexDependencies()
        {
            var tasks = new List<Task> { _taskA, _taskB, _taskC, _taskD };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.IsTrue(result.CriticalPath.Count > 0);
            Assert.IsTrue(result.CriticalTasks.Count > 0);
            Assert.IsTrue(result.CriticalTasks.Any(t => t.Id == 4));

            VerifyCriticalAndNonCriticalTasks(result);
            Assert.IsTrue(result.CriticalPath.Any(t => t.Id == 2) || result.CriticalPath.Any(t => t.Id == 1));
            Assert.IsTrue(result.CriticalPath.Any(t => t.Id == 4));
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
        public void CalculateCriticalPath_ShouldHandleMultipleParallelPaths()
        {
            var taskE = CreateTask("Tarea E", "Descripción de Tarea E", 6, 5);
            var taskF = CreateTask("Tarea F", "Descripción de Tarea F", 2, 6, new List<Task> { taskE });
            var taskG = CreateTask("Tarea G", "Descripción de Tarea G", 1, 7, new List<Task> { _taskD, taskF });

            var tasks = new List<Task> { _taskA, _taskB, _taskC, _taskD, taskE, taskF, taskG };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.IsTrue(result.CriticalPath.Count > 0);
            Assert.IsTrue(result.CriticalTasks.Count > 0);
            Assert.IsTrue(result.CriticalTasks.Any(t => t.Id == 7));

            var criticalIds = result.CriticalTasks.Select(t => t.Id).ToList();
            Assert.IsTrue(criticalIds.Contains(5) && criticalIds.Contains(6));
            Assert.IsTrue(result.ProjectDuration >= 9);
            VerifyCriticalPathOrder(result);
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldHandleParallelTasksWithSameStartEnd()
        {
            var parallelTask1 = CreateTask("Paralela 1", "Descripción", 5, 10);
            var parallelTask2 = CreateTask("Paralela 2", "Descripción", 5, 11);
            var finalTask = CreateTask("Final", "Descripción", 2, 12, new List<Task> { parallelTask1, parallelTask2 });

            var tasks = new List<Task> { parallelTask1, parallelTask2, finalTask };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.AreEqual(3, result.CriticalTasks.Count);
            Assert.AreEqual(7, result.ProjectDuration);

            Assert.IsTrue(result.CriticalTasks.Any(t => t.Id == 10));
            Assert.IsTrue(result.CriticalTasks.Any(t => t.Id == 11));
            Assert.IsTrue(result.CriticalTasks.Any(t => t.Id == 12));
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldHandleCompleteProjectNetwork()
        {
            var task1 = CreateTask("Task 1", "Description", 5, 1);
            var task2 = CreateTask("Task 2", "Description", 3, 2, new List<Task> { task1 });
            var task3 = CreateTask("Task 3", "Description", 4, 3, new List<Task> { task1 });
            var task4 = CreateTask("Task 4", "Description", 2, 4, new List<Task> { task2, task3 });
            var task5 = CreateTask("Task 5", "Description", 3, 5, new List<Task> { task3 });
            var task6 = CreateTask("Task 6", "Description", 1, 6, new List<Task> { task4, task5 });

            var tasks = new List<Task> { task1, task2, task3, task4, task5, task6 };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.AreEqual(6, result.AllTasks.Count);
            Assert.IsTrue(result.CriticalPath.Count > 0);
            Assert.IsTrue(result.CriticalPath.First().PreviousTasks.Count == 0);
            Assert.IsTrue(!IsSuccessorOfAny(result.CriticalPath.Last(), tasks));
            VerifyCriticalPathContinuity(result);
            Assert.AreEqual(13, result.ProjectDuration);
            
            foreach (var criticalTask in result.CriticalTasks)
            {
                Assert.AreEqual(0, criticalTask.Slack.TotalDays, 0.0001);
            }
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldIdentifyCriticalPath_WhenAllCriticalTasksHaveDependencies()
        {
            var taskA = CreateTask("Task A", "Description", 2, 1);
            var taskB = CreateTask("Task B", "Description", 3, 2, new List<Task> { taskA });
            var taskC = CreateTask("Task C", "Description", 4, 3, new List<Task> { taskB });
            var taskD = CreateTask("Task D", "Description", 1, 4, new List<Task> { taskC });

            var tasks = new List<Task> { taskA, taskB, taskC, taskD };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.AreEqual(4, result.CriticalPath.Count);
            VerifyCriticalPathSequence(result, new[] { 1, 2, 3, 4 });
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldHandleTaskWithMultipleCriticalPredecessors()
        {
            var taskX = CreateTask("Task X", "Description", 3, 1);
            var taskY = CreateTask("Task Y", "Description", 4, 2);
            var taskZ = CreateTask("Task Z", "Description", 2, 3, new List<Task> { taskX, taskY });

            var tasks = new List<Task> { taskX, taskY, taskZ };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.IsTrue(result.CriticalPath.Count >= 2);
            Assert.IsTrue(result.CriticalPath.Any(t => t.Id == 2));
            Assert.IsTrue(result.CriticalPath.Any(t => t.Id == 3));
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldHandleNoInitialCriticalTask()
        {
            var taskA = CreateTask("Task A", "Description", 2, 1);
            var taskB = CreateTask("Task B", "Description", 3, 2);
            var taskC = CreateTask("Task C", "Description", 1, 3, new List<Task> { taskA, taskB });

            taskA.StartDate = _defaultStartDate;
            taskB.StartDate = _defaultStartDate;
            taskC.StartDate = new DateTime(2025, 1, 5);

            var tasks = new List<Task> { taskA, taskB, taskC };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.IsTrue(result.CriticalPath.Count > 0);
        }

        [TestMethod]
        public void CalculateCriticalPath_ShouldHandleEdgeCases()
        {
            var taskA = CreateTask("A", "Start", 1, 1);
            var taskB1 = CreateTask("B1", "Branch 1", 2, 2, new List<Task> { taskA });
            var taskB2 = CreateTask("B2", "Branch 2", 3, 3, new List<Task> { taskA });
            var taskC = CreateTask("C", "End", 1, 4, new List<Task> { taskB1, taskB2 });

            var tasks = new List<Task> { taskA, taskB1, taskB2, taskC };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.AreEqual(3, result.CriticalPath.Count);
            Assert.IsTrue(result.CriticalPath.Any(t => t.Id == 1));
            Assert.IsTrue(result.CriticalPath.Any(t => t.Id == 3));
            Assert.IsTrue(result.CriticalPath.Any(t => t.Id == 4));
            Assert.AreEqual(5, result.ProjectDuration);
        }

        #endregion

        #region Project Duration Tests

        [TestMethod]
        public void CalculateProjectDuration_HandlesEmptyList()
        {
            var taskA = CreateTask("Task A", "Description", 3, 1);
            var tasks = new List<Task> { taskA };
            var result = _cpmService.CalculateCriticalPath(tasks);

            Assert.AreEqual(3, result.ProjectDuration);
        }

        #endregion

        #region Helper Methods

        private bool IsSuccessorOfAny(Task task, List<Task> allTasks)
        {
            return allTasks.Any(t => t.PreviousTasks.Contains(task));
        }

        private void VerifyCriticalAndNonCriticalTasks(CpmResult result)
        {
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
        }

        private void VerifyCriticalPathOrder(CpmResult result)
        {
            for (int i = 0; i < result.CriticalPath.Count - 1; i++)
            {
                var currentTask = result.CriticalPath[i];
                var nextTask = result.CriticalPath[i + 1];

                Assert.IsTrue(nextTask.PreviousTasks.Contains(currentTask) ||
                              nextTask.PreviousTasks.Any(p => result.CriticalPath.Contains(p)),
                    $"El camino crítico no está ordenado correctamente: {currentTask.Title} -> {nextTask.Title}");
            }
        }

        private void VerifyCriticalPathContinuity(CpmResult result)
        {
            for (int i = 0; i < result.CriticalPath.Count - 1; i++)
            {
                var current = result.CriticalPath[i];
                var next = result.CriticalPath[i + 1];
                Assert.IsTrue(next.PreviousTasks.Contains(current));
            }
        }

        private void VerifyCriticalPathSequence(CpmResult result, int[] expectedIds)
        {
            for (int i = 0; i < expectedIds.Length; i++)
            {
                Assert.AreEqual(expectedIds[i], result.CriticalPath[i].Id);
            }
        }

        #endregion
    }
}