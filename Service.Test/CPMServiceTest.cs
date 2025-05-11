using Domain;
using Domain.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service;
using System;
using System.Collections.Generic;
using Task = Domain.Task;

namespace Service.Test
{
    [TestClass]
    public class CpmServiceTest
    {
        private CpmService _cpmService;

        [TestInitialize]
        public void Setup()
        {
            _cpmService = new CpmService();
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
    }
}