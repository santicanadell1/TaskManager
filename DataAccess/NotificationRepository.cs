using DataAccess.Exceptions.NotificationRepositoryExceptions;
using Domain;

namespace DataAccess;

public class NotificationRepository
{
    private static int _nextId;
    private readonly List<Notification> _notifications;

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
        var index = -1;


        for (var i = 0; i < _notifications.Count; i++)
            if (_notifications[i] == oldNotification)
            {
                index = i;
                break;
            }

        if (index == -1) throw new NotificationNotFoundException();


        _notifications[index] = newNotification;
    }

    public void Delete(Notification notification)
    {
        var index = -1;

        for (var i = 0; i < _notifications.Count; i++)
            if (_notifications[i] == notification)
            {
                index = i;
                break;
            }

        if (index == -1) throw new NotificationNotFoundException();

        _notifications.RemoveAt(index);
    }
}