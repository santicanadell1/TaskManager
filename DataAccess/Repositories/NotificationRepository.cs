using DataAccess.Exceptions.NotificationRepositoryExceptions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class NotificationRepository : IRepository<Notification>
{
    protected readonly AppDbContext _db;

    public NotificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public List<Notification> GetAll()
    {
        return _db.Set<Notification>().ToList();
    }

    public void Add(Notification notification)
    {
        _db.Set<Notification>().Add(notification);
        _db.SaveChanges();
    }

    public Notification? Get(Func<Notification, bool> filter)
    {
        return _db.Set<Notification>()
            .Include(n => n.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefault(filter);
    }

    public void Update(Notification newNotification)
    {
        try
        {
            var existingNotification = _db.Notifications.Find(newNotification.Id);
            if (existingNotification == null) throw new NotificationNotFoundException();

            existingNotification.Description = newNotification.Description;
            existingNotification.IsRead = newNotification.IsRead;
            existingNotification.Project = newNotification.Project;

            _db.SaveChanges();
        }
        catch (Exception)
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