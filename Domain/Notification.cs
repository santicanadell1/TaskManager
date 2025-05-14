using Domain.Exceptions.NotificationExceptions;

namespace Domain
{
    public class Notification
    {
        public bool read;
        public string description;
        private Project project;

        public bool Read
        {
            get => read;
            set => read = value;
        }

        public string Description
        {
            get => description;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new NotificationDescriptionException();
                }

                description = value;
            }
        }

        public Project Project
        {
            get => project;
            set
            {
                if (value == null)
                {
                    throw new NotificationException("Project is null");
                }
                project = value;
            }
        }
        

        public void MarkRead()
        {
            read = true;
        }

        public int Id { get; set; }


        public Notification(bool read, string description, Project project)
        {
            this.Read = read;
            this.Description = description;
            this.Project = project;
        }
    }
}