using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class OrgTaskConfiguration : IEntityTypeConfiguration<OrgTask>
{
    public void Configure(EntityTypeBuilder<OrgTask> builder)
    {
        builder.ToTable("OrgTasks");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.EventCategoryId)
            .IsRequired(false);

        builder.Property(e => e.TaskName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.AssigneeId)
            .IsRequired(false);

        builder.Property(e => e.DeptId)
            .IsRequired(false);

        builder.Property(e => e.Priority)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Note)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedByMemberId)
            .IsRequired(false);

        builder.Property(e => e.CompletedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(e => e.EventCategoryId);
        builder.HasIndex(e => e.AssigneeId);
        builder.HasIndex(e => e.DeptId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Deadline);

        // Relationships
        builder.HasOne(e => e.EventCategory)
            .WithMany(e => e.Tasks)
            .HasForeignKey(e => e.EventCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Assignee)
            .WithMany(e => e.AssignedTasks)
            .HasForeignKey(e => e.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Department)
            .WithMany(e => e.AssignedTasks)
            .HasForeignKey(e => e.DeptId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CreatedByMember)
            .WithMany(e => e.CreatedTasks)
            .HasForeignKey(e => e.CreatedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
