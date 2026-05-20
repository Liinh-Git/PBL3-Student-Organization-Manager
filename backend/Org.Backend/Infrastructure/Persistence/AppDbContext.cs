using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Infrastructure.Persistence.Configurations;

namespace Org.Backend.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets cho MUST_HAVE_DB_V1 entities
    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventMember> EventMembers => Set<EventMember>();
    public DbSet<Attendee> Attendees => Set<Attendee>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<OrgTask> OrgTasks => Set<OrgTask>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();

    // DbSets cho SHOULD_HAVE_DB_V1_NO_WORKING_UI_YET entities
    public DbSet<DigitalAsset> DigitalAssets => Set<DigitalAsset>();
    public DbSet<EventRating> EventRatings => Set<EventRating>();
    public DbSet<EventReport> EventReports => Set<EventReport>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ActivityHistory> ActivityHistories => Set<ActivityHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply tất cả configurations từ assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filter cho soft-delete (BaseEntity entities)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    ?.MakeGenericMethod(entityType.ClrType);
                
                method?.Invoke(null, new object[] { modelBuilder });
            }
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
