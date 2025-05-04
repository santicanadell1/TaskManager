using DataAccess;
using Domain;
using Service.Models;
using Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Domain.Exceptions;

namespace Service.Test
{
    [TestClass]
    public class TaskServiceTest
    {
        private InMemoryDatabase _database;
        private TaskService _taskService;

        [TestInitialize]
        public void Setup()
        {
            _database = new InMemoryDatabase();
            _taskService = new TaskService(_database);
        }