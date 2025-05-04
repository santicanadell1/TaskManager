using DataAccess;
using Domain;
using Service.Models;
using System.Linq;
using Task = Domain.Task;

namespace Service
{
    public class TaskService
    {
        private readonly InMemoryDatabase _database;

        public TaskService(InMemoryDatabase database)
        {
            _database = database;
            
        }
       


    }
}