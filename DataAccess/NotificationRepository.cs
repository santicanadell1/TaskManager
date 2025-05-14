using Domain;
using DataAccess.Exceptions.NotificationRepositoryExceptions;

namespace DataAccess
{
    public class NotificationRepository
    {
        private readonly List<Notification> _notifications;
        private static int _nextId;

        public NotificationRepository()
        {
            _notifications = new List<Notification>();
            _nextId = 1;
        }

        public List<Notification> GetAll()
        {
            return _notifications.ToList();
        }

        public void AddNotification(Notification notification)
        {
            notification.Id = _nextId++;
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
                throw new NotificationNotFoundException();
            }


            _notifications[index] = newNotification;
        }

        public void Delete(Notification notification)
        {
            int index = -1;

            for (int i = 0; i < _notifications.Count; i++)
            {
                if (_notifications[i] == notification)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                throw new NotificationNotFoundException();
            }

            _notifications.RemoveAt(index);
        }
    }
}