using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class AttendeeConfiguration : IEntityTypeConfiguration<Attendee>
{
    public void Configure(EntityTypeBuilder<Attendee> builder)
    {
        builder.ToTable("Attendees");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.UserId)
            .IsRequired(false);

        builder.Property(e => e.GuestName)
            .HasMaxLength(200);

        builder.Property(e => e.GuestEmail)
            .HasMaxLength(256);

        builder.Property(e => e.GuestPhone)
            .HasMaxLength(20);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.RegisteredAt)
            .IsRequired()
            .HasDefaultValueSql("now() AT TIME ZONE 'UTC'");

        builder.Property(e => e.Note)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.EventId);
        builder.HasIndex(e => e.UserId);

        // Relationships
        builder.HasOne(e => e.Event)
            .WithMany(e => e.Attendees)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany(e => e.Attendees)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
