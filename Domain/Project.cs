using Domain.Exceptions.ProjectExceptions;

namespace Domain
{
    public class Project
    {
        private string name;
        private string description;
        private DateTime startDate;

        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ProjectNameException();
                }

                name = value;
            }
        }

        public string Description
        {
            get => description;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ProjectDescriptionException();
                }

                description = value;
            }
        }

        public DateTime StartDate
        {
            get => startDate;
            set
            {
                if (value == default(DateTime))
                {
                    throw new ProjectStartDateException();
                }

                if (value < DateTime.Today)
                {
                    throw new ProjectException("The Start Date must be greater than the current date");
                }

                startDate = value;
            }
        }

        public List<User> Members { get; set; } = new List<User>();
        public List<Task> Tasks { get; set; } = new List<Task>();
        public User AdminProject { get; set; }

        public Project()
        {
        }

        public Project(string name, string description, DateTime startDate)
        {
            Name = name;
            Description = description;
            StartDate = startDate;
        }

        public void AddMember(User user)
        {
            Members.Add(user);
        }

        public void AddTask(Task task)
        {
            Tasks.Add(task);
        }
        
    }
}