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
        
    }
    
}