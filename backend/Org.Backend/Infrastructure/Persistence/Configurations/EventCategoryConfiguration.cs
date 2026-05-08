using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class EventCategoryConfiguration : IEntityTypeConfiguration<EventCategory>
{
    public void Configure(EntityTypeBuilder<EventCategory> builder)
    {
        builder.ToTable("EventCategories");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.MilestoneId)
            .IsRequired();

        builder.Property(e => e.CategoryName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.OrderIndex)
            .IsRequired();

        builder.Property(e => e.OwnerDepartmentId)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(e => e.MilestoneId);
        builder.HasIndex(e => e.OwnerDepartmentId);
        builder.HasIndex(e => new { e.MilestoneId, e.OrderIndex });

        // Relationships
        builder.HasOne(e => e.Milestone)
            .WithMany(e => e.Categories)
            .HasForeignKey(e => e.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.OwnerDepartment)
            .WithMany(e => e.OwnedCategories)
            .HasForeignKey(e => e.OwnerDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Tasks)
            .WithOne(e => e.EventCategory)
            .HasForeignKey(e => e.EventCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
