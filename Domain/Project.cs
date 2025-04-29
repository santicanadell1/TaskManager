using Domain.Exceptions;

namespace Domain
{
    public class Project
    {
        public string name;
        public string description;
        public DateTime startDate;
        public List<User> members;
        public List<Task> tasks;
        public List<Notification> notifications;
        public User adminProject;

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
                startDate = value;
            }
        }

        public List<User> Members { get; set; }
        public List<Task> Tasks { get; set; }
        
        public List<Notification> Notifications { get; set; }
        public User AdminProject { get; set; }

        public Project()
        {
            this.Members = new List<User>();
            this.Tasks = new List<Task>();
            this.Notifications = new List<Notification>();
        }
        
        public void AddMember(User user)
        {
            this.Members.Add(user);
        }
        public void AddTask(Task task)
        {
            this.Tasks.Add(task);
        }
       
    }
}