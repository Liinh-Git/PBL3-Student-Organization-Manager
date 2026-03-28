using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── DbSets ──────────────────────────────────────────────────────────────────
    // Module: Người dùng
    public DbSet<User> Users => Set<User>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

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
    public DbSet<OrgTask> Tasks => Set<OrgTask>();
    public DbSet<Attendee> Attendees => Set<Attendee>();
    public DbSet<DigitalAsset> DigitalAssets => Set<DigitalAsset>();

    // ── Auto-set UpdatedAt on every save ────────────────────────────────────────
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
        });

        // ── User ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Status).HasConversion<int>();
        });

        // ── Role ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<Role>(e =>
        {
            e.HasOne(r => r.Organization)
             .WithMany(o => o.Roles)
             .HasForeignKey(r => r.OrgId)
             .OnDelete(DeleteBehavior.Cascade);
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

        // ── OrgTask ───────────────────────────────────────────────────────────────
        modelBuilder.Entity<OrgTask>(e =>
        {
            e.HasOne(t => t.Milestone)
             .WithMany(m => m.Tasks)
             .HasForeignKey(t => t.MilestoneId)
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

        // ── Global soft-delete query filter for all BaseEntity subclasses ─────────
        ApplySoftDeleteFilters(modelBuilder);
    }

    /// <summary>
    /// Applies a global HasQueryFilter(e => !e.IsDeleted) to every entity that
    /// inherits from BaseEntity, so soft-deleted rows are invisible by default.
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
