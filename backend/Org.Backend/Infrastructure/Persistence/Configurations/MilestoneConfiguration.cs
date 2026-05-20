using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.ToTable("Milestones");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.OrderIndex)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        // Indexes
        builder.HasIndex(e => e.EventId);
        builder.HasIndex(e => new { e.EventId, e.OrderIndex });

        // Relationships
        builder.HasOne(e => e.Event)
            .WithMany(e => e.Milestones)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Categories)
            .WithOne(e => e.Milestone)
            .HasForeignKey(e => e.MilestoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
