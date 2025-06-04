using Domain;
using Microsoft.EntityFrameworkCore;
using Task = Domain.Task;

namespace DataAccess;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        if (!Database.IsInMemory())
        {
            Database.Migrate();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Notifications)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserNotifications",
                j => j.HasOne<Notification>().WithMany().OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<User>().WithMany().OnDelete(DeleteBehavior.ClientCascade));

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Project)
            .WithMany()
            .HasForeignKey("ProjectId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Members)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ProjectMembers",
                j => j.HasOne<User>().WithMany().OnDelete(DeleteBehavior.ClientCascade),
                j => j.HasOne<Project>().WithMany().OnDelete(DeleteBehavior.Cascade));

        modelBuilder.Entity<User>()
            .HasMany(u => u.Tasks)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserTasks",
                j => j.HasOne<Domain.Task>().WithMany().OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<User>().WithMany().OnDelete(DeleteBehavior.ClientCascade));
        
        modelBuilder.Entity<Project>()
            .HasOne(p => p.AdminProject)
            .WithMany()
            .HasForeignKey("AdminProjectId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Domain.Task>()
            .HasOne<Project>()
            .WithMany(p => p.Tasks)
            .HasForeignKey("ProjectId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Domain.Task>()
            .HasMany(t => t.Resources)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TaskResources",
                j => j.HasOne<Resource>().WithMany().OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Domain.Task>().WithMany().OnDelete(DeleteBehavior.Cascade));

        modelBuilder.Entity<Domain.Task>()
            .HasMany(t => t.PreviousTasks)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TaskDependencies",
                j => j.HasOne<Domain.Task>().WithMany().HasForeignKey("PreviousTaskId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<Domain.Task>().WithMany().HasForeignKey("DependentTaskId").OnDelete(DeleteBehavior.Restrict));

        modelBuilder.Entity<Domain.Task>()
            .HasMany(t => t.SameTimeTasks)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ConcurrentTasks",
                j => j.HasOne<Domain.Task>().WithMany().HasForeignKey("TaskId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<Domain.Task>().WithMany().HasForeignKey("ConcurrentTaskId").OnDelete(DeleteBehavior.Restrict));

    }
}