using Domain;
namespace DataAccess
{
    public class NotificationRepository
    {
        private readonly List<Notification> _notifications;

        public NotificationRepository()
        {
            _notifications = new List<Notification>();
        }
        
        public List<Notification> GetAll()
        {
            return _notifications.ToList();
        }

        public void AddNotification(Notification notification)
        {
            _notifications.Add(notification);
        }
        
        public Notification? Get(Func<Notification, bool> filter)
        {
            return _notifications.FirstOrDefault(filter);
        }
        
        public void Update(Notification oldNotification, Notification newNotification)
        {
            
            int index = -1;


            for (int i = 0; i < _notifications.Count; i++)
            {

                if (_notifications[i] == oldNotification)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                throw new ArgumentException("Notification not found");
           }
            
            _notifications[index] = newNotification;
        }
        
    }
    
}