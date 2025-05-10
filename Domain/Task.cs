using Domain.Exceptions;
using System;
using System.Collections.Generic;

namespace Domain
{
    public class Task
    {
        private string _title;
        private string _description;
        private DateTime _expectedStartDate;
        private DateTime _startDate;
        private DateTime _endDate;
        private int _duration;
        private List<Task> _previousTasks;
        private List<Task> _sameTimeTasks;
        private State _state;
        private List<Resource> _resources;

        // Propiedades para CPM
        private DateTime _latestStart;
        private DateTime _latestFinish;

        public Task(string title, string description, DateTime startDate, int duration, List<Task> previousTasks,
            List<Task> sameTimeTasks, List<Resource> resources)
        {
            this.Title = title;
            this.Description = description;
            this.ExpectedStartDate = startDate;
            this.Duration = duration;
            this.PreviousTasks = previousTasks ?? new List<Task>();
            this.SameTimeTasks = sameTimeTasks ?? new List<Task>();
            this.State = State.TODO;
            this.Resource = resources ?? new List<Resource>();

            // Inicializar propiedades CPM
            this.StartDate = startDate;
            this.EndDate = startDate.AddDays(duration);
            this.LatestStart = startDate;
            this.LatestFinish = startDate.AddDays(duration);
            
        }

        public List<Resource> Resource
        {
            get => _resources;
            set
            {
                if (value == null)
                {
                    throw new TaskResourceException("Resources cannot be null ");
                }

                _resources = value;
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new TaskTitleException();
                }

                _title = value;
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new TaskDescriptionException();
                }

                _description = value;
            }
        }

        public DateTime ExpectedStartDate
        {
            get => _expectedStartDate;
            set => _expectedStartDate = value;
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => _startDate = value;
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => _endDate = value;
        }

        public DateTime LatestStart
        {
            get => _latestStart;
            set => _latestStart = value;
        }
        public DateTime LatestFinish
        {
            get => _latestFinish;
            set => _latestFinish = value;
        }
        
        public int Duration
        {
            get => _duration;
            set
            {
                if (value <= 0)
                {
                    throw new TaskDurationException();
                }

                _duration = value;
            }
        }

        public List<Task> PreviousTasks
        {
            get => _previousTasks;
            set
            {
                if (value == null)
                {
                    throw new TaskPreviousTaskException("PreviousTasks cannot be null ");
                }

                _previousTasks = value;
            }
        }

        public void AddPreviousTask(Task task)
        {
            if (task == null)
            {
                throw new TaskResourceException("Task cannot be null.");
            }

            PreviousTasks.Add(task);
        }

        public void RemovePreviousTask(Task task)
        {
            if (task == null)
            {
                throw new TaskResourceException("Task cannot be null.");
            }

            PreviousTasks.Remove(task);
        }

        public List<Task> SameTimeTasks
        {
            get => _sameTimeTasks;
            set => _sameTimeTasks = value ?? new List<Task>();
        }

        public void AddSameTimeTask(Task task)
        {
            if (task == null)
            {
                throw new TaskResourceException("Task cannot be null.");
            }

            SameTimeTasks.Add(task);
        }

        public void RemoveSameTimeTask(Task task)
        {
            if (task == null)
            {
                throw new TaskResourceException("Task cannot be null.");
            }

            SameTimeTasks.Remove(task);
        }

        public State State { get; set; }

        public int? Id { get; set; }
    }
}