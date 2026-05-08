using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.OrgId)
            .IsRequired();

        builder.Property(e => e.DepartmentId)
            .IsRequired(false);

        builder.Property(e => e.RoleId)
            .IsRequired(false);

        builder.Property(e => e.JoinDate)
            .IsRequired()
            .HasDefaultValueSql("now() AT TIME ZONE 'UTC'");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.StudentCode)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(e => new { e.UserId, e.OrgId })
            .IsUnique();

        builder.HasIndex(e => e.OrgId);
        builder.HasIndex(e => e.DepartmentId);
        builder.HasIndex(e => e.RoleId);

        // Relationships
        builder.HasOne(e => e.User)
            .WithMany(e => e.Members)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Organization)
            .WithMany(e => e.Members)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany(e => e.Members)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Role)
            .WithMany(e => e.Members)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.ManagedDepartments)
            .WithOne(e => e.Manager)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.AssignedTasks)
            .WithOne(e => e.Assignee)
            .HasForeignKey(e => e.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.CreatedTasks)
            .WithOne(e => e.CreatedByMember)
            .HasForeignKey(e => e.CreatedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.EventMemberships)
            .WithOne(e => e.Member)
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ReviewedRequests)
            .WithOne(e => e.ReviewedByMember)
            .HasForeignKey(e => e.ReviewedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.EventReports)
            .WithOne(e => e.CreatedByMember)
            .HasForeignKey(e => e.CreatedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
