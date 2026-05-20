using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class EventMemberConfiguration : IEntityTypeConfiguration<EventMember>
{
    public void Configure(EntityTypeBuilder<EventMember> builder)
    {
        builder.ToTable("EventMembers");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.MemberId)
            .IsRequired();

        builder.Property(e => e.EventRole)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.AssignedAt)
            .IsRequired()
            .HasDefaultValueSql("now() AT TIME ZONE 'UTC'");

        builder.Property(e => e.Note)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => new { e.EventId, e.MemberId })
            .IsUnique();

        builder.HasIndex(e => e.MemberId);
        builder.HasIndex(e => e.EventRole);

        // Relationships
        builder.HasOne(e => e.Event)
            .WithMany(e => e.EventMembers)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Member)
            .WithMany(e => e.EventMemberships)
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
