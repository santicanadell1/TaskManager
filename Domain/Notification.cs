using Domain.Exceptions;

namespace Domain
{
    public class Notification
    {
        public bool read;
        public string description;

        public bool Read { get => read; set => read = value; }

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

        public void MarkRead()
        {
            read = true;
        }

        public Notification(bool read, string description)
        {
            this.Read = read;
            this.Description = description;
        }
        
    }
}