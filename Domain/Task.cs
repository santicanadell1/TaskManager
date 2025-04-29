using Domain.Exceptions;
using Domain.Test;

namespace Domain
{
    public class Task
    {
        private string title;
        private string description;
        private DateTime expectedStartDate;
        private DateTime expectedEndDate;
        private DateTime startDate;
        private DateTime endDate;
        private int duration;
        private List<Task> previousTasks;
        private State state;

        public Task(string title, string description, DateTime startDate, DateTime expectedEndDate, int duration, List<Task> previousTasks)
        {
            this.Title = title;  // Ensures the Title is validated
            this.Description = description;  // Ensures the Description is validated
            this.ExpectedStartDate = startDate;
            this.ExpectedEndDate = expectedEndDate;
            this.Duration = duration;  // This will validate the Duration value
            this.PreviousTasks = previousTasks;
            this.State = State.TODO;  // Default state to TODO
        }

        public string Title
        {
            get => title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new TaskTitleException();
                }
                title = value;
            }
        }

        public string Description
        {
            get => description;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new TaskDescriptionException();
                }
                description = value;
            }
        }

        public DateTime ExpectedStartDate { get; set; }

        public DateTime ExpectedEndDate
        {
            get => expectedEndDate;
            set
            {
                if (value < this.ExpectedStartDate)
                {
                    throw new TaskEndDateException();
                }
                expectedEndDate = value;
            }
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int Duration
        {
            get => duration;
            set
            {
                if (value <= 0)
                {
                    throw new TaskDurationException(); // This is where the exception is raised for invalid duration
                }
                duration = value;
            }
        }

        public List<Task> PreviousTasks { get; set; }
        public State State { get; set; }
    }
}
