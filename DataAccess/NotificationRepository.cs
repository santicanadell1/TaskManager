using DataAccess.Exceptions.NotificationRepositoryExceptions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class NotificationRepository
{
    private static int _nextId;
    protected readonly AppDbContext _db;

    public NotificationRepository(AppDbContext db)
    {
        _db = db;
        _nextId = 1;
    }

    public List<Notification> GetAll()
    {
        return _db.Set<Notification>().ToList();
    }

    public void Add(Notification notification)
    {
        notification.Id = _nextId++;
        _db.Set<Notification>().Add(notification);
        _db.SaveChanges();
    }

    public Notification? Get(Func<Notification, bool> filter)
    {
        return _db.Set<Notification>().FirstOrDefault(filter);
    }

    public void Update(Notification oldNotification, Notification newNotification)
    {
        try
        {
            var existingNotification = _db.Notifications.Find(oldNotification.Id);
            _db.Entry(existingNotification).CurrentValues.SetValues(newNotification);
            _db.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new NotificationNotFoundException();
        }
    }


    public void Delete(Notification notification)
    {
        try
        {
            _db.Set<Notification>().Remove(notification);
            _db.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new NotificationNotFoundException();
        }
    }
}