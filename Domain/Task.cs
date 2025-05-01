using Domain.Exceptions;
using Domain.Test;

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

        public Task(string title, string description, DateTime startDate, int duration, List<Task> previousTasks)
        {
            this.Title = title; 
            this.Description = description; 
            this.ExpectedStartDate = startDate;
            this.Duration = duration;  
            this.PreviousTasks = previousTasks;
            this.State = State.TODO;  
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

        public DateTime ExpectedStartDate { get; set; }

        

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

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

        public List<Task> PreviousTasks { get; set; }

        public void AddPreviousTask(Task task)
        {
            PreviousTasks.Add(task);
        }

        public void RemovePreviousTask(Task task)
        {
            PreviousTasks.Remove(task);
        }
        public State State { get; set; }
    }
}
