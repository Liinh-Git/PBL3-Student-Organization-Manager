using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resources");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.OrgId)
            .IsRequired();

        builder.Property(e => e.EventId)
            .IsRequired(false);

        builder.Property(e => e.ResourceName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Type)
            .HasMaxLength(100);

        builder.Property(e => e.Quantity)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Note)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.OrgId);
        builder.HasIndex(e => e.EventId);
        builder.HasIndex(e => e.Status);

        // Relationships
        builder.HasOne(e => e.Organization)
            .WithMany(e => e.Resources)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Event)
            .WithMany(e => e.Resources)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
