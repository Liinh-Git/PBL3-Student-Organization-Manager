// ---- AppDbContext: khai báo DbSet và toàn bộ mapping/constraint cho database ----
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Database;

public class AppDbContext : DbContext
{
    // ---- Constructor nhận DbContextOptions từ DI ----
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── DbSets ──────────────────────────────────────────────────────────────────
    // Module: Người dùng
    public DbSet<User> Users => Set<User>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();

    // Module: Tổ chức
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<ActivityHistory> ActivityHistories => Set<ActivityHistory>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<Resource> Resources => Set<Resource>();

    // Module: Sự kiện
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventMember> EventMembers => Set<EventMember>();
    public DbSet<EventReport> EventReports => Set<EventReport>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<OrgTask> Tasks => Set<OrgTask>();
    public DbSet<Attendee> Attendees => Set<Attendee>();
    public DbSet<DigitalAsset> DigitalAssets => Set<DigitalAsset>();
    public DbSet<EventRating> EventRatings => Set<EventRating>();

    // Module: Bài viết
    public DbSet<OrganizationPost> OrganizationPosts => Set<OrganizationPost>();

    // Module: Thông báo
    public DbSet<Notification> Notifications => Set<Notification>();

    // ---- Tự động set CreatedAt/UpdatedAt cho các entity kế thừa BaseEntity ----
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    // ---- Khai báo toàn bộ quan hệ FK, index, enum conversion, và check constraint ----
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Table name overrides ────────────────────────────────────────────────
        modelBuilder.Entity<OrgTask>().ToTable("Tasks");

        // ── RolePermission — composite PK (no BaseEntity) ─────────────────────
        modelBuilder.Entity<RolePermission>(e =>
        {
            e.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            e.HasOne(rp => rp.Role)
             .WithMany(r => r.RolePermissions)
             .HasForeignKey(rp => rp.RoleId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(rp => rp.Permission)
             .WithMany(p => p.RolePermissions)
             .HasForeignKey(rp => rp.PermissionId)
             .OnDelete(DeleteBehavior.Cascade);

            // Keep required relationship behavior consistent with soft-delete filters
            // on Role/Permission to avoid required-navigation/filter mismatch warnings.
            e.HasQueryFilter(rp => !rp.Role.IsDeleted && !rp.Permission.IsDeleted);
        });

        // ── User ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Status).HasConversion<int>();
            e.Property(u => u.ProfileVisibility).HasConversion<int>();
        });

        // ── FriendRequest ───────────────────────────────────────────────────────
        modelBuilder.Entity<FriendRequest>(e =>
        {
            e.HasOne(fr => fr.Sender)
             .WithMany(u => u.SentFriendRequests)
             .HasForeignKey(fr => fr.SenderId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(fr => fr.Receiver)
             .WithMany(u => u.ReceivedFriendRequests)
             .HasForeignKey(fr => fr.ReceiverId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(fr => fr.Status).HasConversion<int>();
            
            // Prevent duplicate friend requests
            e.HasIndex(fr => new { fr.SenderId, fr.ReceiverId, fr.Status });
        });

        // ── Role ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<Role>(e =>
        {
            e.HasOne(r => r.Organization)
             .WithMany(o => o.Roles)
             .HasForeignKey(r => r.OrgId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(r => new { r.OrgId, r.RoleName }).IsUnique();
        });

        // ── Permission ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Permission>(e =>
        {
            e.HasIndex(p => p.PermissionKey).IsUnique();
        });

        // ── Organization ────────────────────────────────────────────────────────
        modelBuilder.Entity<Organization>(e =>
        {
            e.Property(o => o.Status).HasConversion<int>();
        });

        // ── Department ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Department>(e =>
        {
            e.HasOne(d => d.Organization)
             .WithMany(o => o.Departments)
             .HasForeignKey(d => d.OrgId)
             .OnDelete(DeleteBehavior.Cascade);

            // managerId → Member (Restrict to avoid cycle)
            e.HasOne(d => d.Manager)
             .WithMany()
             .HasForeignKey(d => d.ManagerId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Member ───────────────────────────────────────────────────────────────
        modelBuilder.Entity<Member>(e =>
        {
            // A user can only be a member of each org once
            e.HasIndex(m => new { m.UserId, m.OrgId }).IsUnique();

            e.HasOne(m => m.User)
             .WithMany(u => u.Members)
             .HasForeignKey(m => m.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Organization)
             .WithMany(o => o.Members)
             .HasForeignKey(m => m.OrgId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Department)
             .WithMany(d => d.Members)
             .HasForeignKey(m => m.DepartmentId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(m => m.Role)
             .WithMany(r => r.Members)
             .HasForeignKey(m => m.RoleId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── ActivityHistory ──────────────────────────────────────────────────────
        modelBuilder.Entity<ActivityHistory>(e =>
        {
            e.HasOne(a => a.Organization)
             .WithMany(o => o.ActivityHistories)
             .HasForeignKey(a => a.OrgId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(a => a.Type).HasConversion<int>();
        });

        // ── Request ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Request>(e =>
        {
            e.HasOne(r => r.Sender)
             .WithMany(u => u.Requests)
             .HasForeignKey(r => r.SenderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.Organization)
             .WithMany(o => o.Requests)
             .HasForeignKey(r => r.OrgId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(r => r.RequestType).HasConversion<int>();
            e.Property(r => r.Status).HasConversion<int>();
        });

        // ── Event ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<Event>(e =>
        {
            e.HasOne(ev => ev.Organization)
             .WithMany(o => o.Events)
             .HasForeignKey(ev => ev.OrgId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(ev => ev.Budget).HasColumnType("numeric(15,2)");
            e.Property(ev => ev.Status).HasConversion<int>();
            e.Property(ev => ev.Visibility).HasConversion<int>();
        });

        // ── EventRating ──────────────────────────────────────────────────────────
        modelBuilder.Entity<EventRating>(e =>
        {
            e.HasOne(er => er.Event)
             .WithMany(ev => ev.Ratings)
             .HasForeignKey(er => er.EventId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(er => er.User)
             .WithMany(u => u.EventRatings)
             .HasForeignKey(er => er.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(er => er.Aspect).HasConversion<int>();
            
            // Unique constraint: một user chỉ đánh giá một aspect của event một lần
            e.HasIndex(er => new { er.EventId, er.UserId, er.Aspect }).IsUnique();
        });

        // ── OrganizationPost ─────────────────────────────────────────────────────
        modelBuilder.Entity<OrganizationPost>(e =>
        {
            e.HasOne(p => p.Organization)
             .WithMany(o => o.Posts)
             .HasForeignKey(p => p.OrgId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.Creator)
             .WithMany(m => m.CreatedPosts)
             .HasForeignKey(p => p.CreatedBy)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.TargetDepartment)
             .WithMany(d => d.TargetedPosts)
             .HasForeignKey(p => p.TargetDepartmentId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(p => p.RelatedEvent)
             .WithMany(ev => ev.RelatedPosts)
             .HasForeignKey(p => p.RelatedEventId)
             .OnDelete(DeleteBehavior.SetNull);

            e.Property(p => p.PostType).HasConversion<int>();
            e.Property(p => p.Visibility).HasConversion<int>();
            
            e.HasIndex(p => new { p.OrgId, p.CreatedAt });
            e.HasIndex(p => new { p.Visibility, p.CreatedAt });
        });

        // ── Resource ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Resource>(e =>
        {
            e.HasOne(r => r.Organization)
             .WithMany(o => o.Resources)
             .HasForeignKey(r => r.OrgId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.Event)
             .WithMany(ev => ev.Resources)
             .HasForeignKey(r => r.EventId)
             .OnDelete(DeleteBehavior.SetNull);

            e.Property(r => r.Status).HasConversion<int>();

            e.ToTable(t => t.HasCheckConstraint("CHK_Resource_Quantity", "\"Quantity\" >= 0"));
        });

        // ── EventMember ───────────────────────────────────────────────────────────
        modelBuilder.Entity<EventMember>(e =>
        {
            e.HasOne(em => em.Event)
             .WithMany(ev => ev.EventMembers)
             .HasForeignKey(em => em.EventId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(em => em.Member)
             .WithMany(m => m.EventMembers)
             .HasForeignKey(em => em.MemberId)
             .OnDelete(DeleteBehavior.Restrict);

              e.HasIndex(em => new { em.EventId, em.MemberId }).IsUnique();
        });

        // ── EventReport (1:1 with Event) ─────────────────────────────────────────
        modelBuilder.Entity<EventReport>(e =>
        {
            e.HasOne(er => er.Event)
             .WithOne(ev => ev.EventReport)
             .HasForeignKey<EventReport>(er => er.EventId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(er => er.ActualBudget).HasColumnType("numeric(15,2)");
        });

        // ── Milestone ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Milestone>(e =>
        {
            e.HasOne(m => m.Event)
             .WithMany(ev => ev.Milestones)
             .HasForeignKey(m => m.EventId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(m => m.Status).HasConversion<int>();
        });

        // ── EventCategory ──────────────────────────────────────────────────────
        modelBuilder.Entity<EventCategory>(e =>
        {
            e.HasOne(c => c.Milestone)
             .WithMany(m => m.Categories)
             .HasForeignKey(c => c.MilestoneId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.OwnerDepartment)
             .WithMany(d => d.OwnedEventCategories)
             .HasForeignKey(c => c.OwnerDepartmentId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(c => new { c.MilestoneId, c.CategoryName }).IsUnique();
            e.HasIndex(c => new { c.MilestoneId, c.OrderIndex });
        });

        // ── OrgTask ───────────────────────────────────────────────────────────────
        modelBuilder.Entity<OrgTask>(e =>
        {
            e.HasOne(t => t.EventCategory)
             .WithMany(c => c.Tasks)
             .HasForeignKey(t => t.EventCategoryId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(t => t.Assignee)
             .WithMany(m => m.AssignedTasks)
             .HasForeignKey(t => t.AssigneeId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.Department)
             .WithMany(d => d.Tasks)
             .HasForeignKey(t => t.DeptId)
             .OnDelete(DeleteBehavior.SetNull);

            e.Property(t => t.Status).HasConversion<int>();
            e.Property(t => t.Priority).HasConversion<int>();
        });

        // ── Attendee ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Attendee>(e =>
        {
            e.HasOne(a => a.Event)
             .WithMany(ev => ev.Attendees)
             .HasForeignKey(a => a.EventId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.User)
             .WithMany(u => u.Attendees)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.SetNull);

            e.Property(a => a.Status).HasConversion<int>();
        });

        // ── DigitalAsset ──────────────────────────────────────────────────────────
        modelBuilder.Entity<DigitalAsset>(e =>
        {
            e.HasOne(da => da.Event)
             .WithMany(ev => ev.DigitalAssets)
             .HasForeignKey(da => da.EventId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(da => da.Uploader)
             .WithMany(m => m.UploadedAssets)
             .HasForeignKey(da => da.UploadedBy)
             .OnDelete(DeleteBehavior.SetNull);

            e.Property(da => da.FileType).HasConversion<int>();
        });

        // ── Notification ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasOne(n => n.Receiver)
             .WithMany(u => u.ReceivedNotifications)
             .HasForeignKey(n => n.ReceiverId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.Actor)
             .WithMany(u => u.CreatedNotifications)
             .HasForeignKey(n => n.ActorId)
             .OnDelete(DeleteBehavior.SetNull);

            e.Property(n => n.Type).HasConversion<int>();
            
            // Index cho query thông báo của user (quan trọng cho performance)
            e.HasIndex(n => new { n.ReceiverId, n.CreatedAt });
            e.HasIndex(n => new { n.ReceiverId, n.IsRead });
            e.HasIndex(n => new { n.ReceiverId, n.Type });
        });

        // ---- Áp dụng soft-delete filter cho mọi entity kế thừa BaseEntity ----
        ApplySoftDeleteFilters(modelBuilder);
    }

    /// <summary>
    /// Duyệt toàn bộ entity kế thừa BaseEntity và gắn HasQueryFilter(!IsDeleted)
    /// để dữ liệu soft-delete không xuất hiện trong truy vấn mặc định.
    /// </summary>
    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(t => !t.ClrType.IsAbstract && typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            var param = Expression.Parameter(entityType.ClrType, "e");
            var prop = Expression.Property(param, nameof(BaseEntity.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(prop), param);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}
