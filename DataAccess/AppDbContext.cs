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

        // Configuración de User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FirstName).IsRequired();
            entity.Property(u => u.LastName).IsRequired();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.Birthday).IsRequired();
            entity.Property(u => u.Password).IsRequired();

            // Configurar Roles como string delimitado con ValueComparer
            entity.Property(u => u.Roles)
                .HasConversion(
                    v => string.Join(',', v.Select(r => r.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Enum.Parse<Rol>(s))
                        .ToList(),
                    new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<Rol>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );
        });

        // Configuración de Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Description).IsRequired();
            entity.Property(p => p.StartDate).IsRequired();

            // Relación con AdminProject
            entity.HasOne(p => p.AdminProject)
                .WithMany()
                .HasForeignKey("AdminProjectId")
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Task
        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired();
            entity.Property(t => t.Description).IsRequired();
            entity.Property(t => t.Duration).IsRequired();
            entity.Property(t => t.ExpectedStartDate).IsRequired();
            entity.Property(t => t.StartDate).IsRequired();
            entity.Property(t => t.EndDate).IsRequired();
            entity.Property(t => t.LatestStart).IsRequired();
            entity.Property(t => t.LatestFinish).IsRequired();
            entity.Property(t => t.Slack).IsRequired();
            entity.Property(t => t.IsCritical).IsRequired();

            // Configurar enum State
            entity.Property(t => t.State)
                .HasConversion<int>();

            // Relación con Project
            entity.HasOne<Project>()
                .WithMany(p => p.Tasks)
                .HasForeignKey("ProjectId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de Resource
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired();
            entity.Property(r => r.Type).IsRequired();
            entity.Property(r => r.Description).IsRequired();
        });

        // Configuración de Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Description).IsRequired();
            entity.Property(n => n.IsRead).IsRequired();

            // Relación con Project
            entity.HasOne(n => n.Project)
                .WithMany()
                .HasForeignKey("ProjectId")
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Relaciones Many-to-Many

        // User - Notifications
        modelBuilder.Entity<User>()
            .HasMany(u => u.Notifications)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserNotifications",
                j => j.HasOne<Notification>().WithMany().OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<User>().WithMany().OnDelete(DeleteBehavior.NoAction));

        // Project - Members
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Members)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ProjectMembers",
                j => j.HasOne<User>().WithMany().OnDelete(DeleteBehavior.NoAction),
                j => j.HasOne<Project>().WithMany().OnDelete(DeleteBehavior.Cascade));

        // User - Tasks
        modelBuilder.Entity<User>()
            .HasMany(u => u.Tasks)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserTasks",
                j => j.HasOne<Task>().WithMany().OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<User>().WithMany().OnDelete(DeleteBehavior.NoAction));

        // Task - Resources
        modelBuilder.Entity<Task>()
            .HasMany(t => t.Resources)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TaskResources",
                j => j.HasOne<Resource>().WithMany().OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Task>().WithMany().OnDelete(DeleteBehavior.Cascade));

        // Task Dependencies (Self-referencing)
        modelBuilder.Entity<Task>()
            .HasMany(t => t.PreviousTasks)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TaskDependencies",
                j => j.HasOne<Task>().WithMany().HasForeignKey("PreviousTaskId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<Task>().WithMany().HasForeignKey("DependentTaskId").OnDelete(DeleteBehavior.Restrict));

        // Concurrent Tasks (Self-referencing)
        modelBuilder.Entity<Task>()
            .HasMany(t => t.SameTimeTasks)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ConcurrentTasks",
                j => j.HasOne<Task>().WithMany().HasForeignKey("TaskId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<Task>().WithMany().HasForeignKey("ConcurrentTaskId").OnDelete(DeleteBehavior.Restrict));
    }
}