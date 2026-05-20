using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class ActivityHistoryConfiguration : IEntityTypeConfiguration<ActivityHistory>
{
    public void Configure(EntityTypeBuilder<ActivityHistory> builder)
    {
        builder.ToTable("ActivityHistories");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.OrgId)
            .IsRequired();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.ReferenceType)
            .HasMaxLength(100);

        builder.Property(e => e.IsPublic)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(e => e.OrgId);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.CreatedAt);

        // Relationships
        builder.HasOne(e => e.Organization)
            .WithMany(e => e.ActivityHistories)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
