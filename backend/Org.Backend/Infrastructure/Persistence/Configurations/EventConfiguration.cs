using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.OrgId)
            .IsRequired();

        builder.Property(e => e.EventName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.StartDate)
            .IsRequired();

        builder.Property(e => e.EndDate)
            .IsRequired();

        builder.Property(e => e.Budget)
            .HasColumnType("numeric(18,2)")
            .IsRequired(false);

        builder.Property(e => e.Location)
            .HasMaxLength(500);

        builder.Property(e => e.Tags)
            .HasColumnType("jsonb");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Visibility)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.CreatedByMemberId)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(e => e.OrgId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Visibility);
        builder.HasIndex(e => e.StartDate);

        // Relationships
        builder.HasOne(e => e.Organization)
            .WithMany(e => e.Events)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreatedByMember)
            .WithMany()
            .HasForeignKey(e => e.CreatedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Milestones)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.EventMembers)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Attendees)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.DigitalAssets)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.EventRatings)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.EventReport)
            .WithOne(e => e.Event)
            .HasForeignKey<EventReport>(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Resources)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
