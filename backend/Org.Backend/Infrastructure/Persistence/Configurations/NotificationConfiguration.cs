using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.ReceiverId)
            .IsRequired();

        builder.Property(e => e.ActorId)
            .IsRequired(false);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.RelatedEntityType)
            .HasMaxLength(100);

        builder.Property(e => e.ActionUrl)
            .HasMaxLength(500);

        builder.Property(e => e.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.ReadAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(e => new { e.ReceiverId, e.IsRead });
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.Type);

        // Relationships
        builder.HasOne(e => e.Receiver)
            .WithMany(e => e.NotificationsReceived)
            .HasForeignKey(e => e.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Actor)
            .WithMany(e => e.NotificationsActedAsActor)
            .HasForeignKey(e => e.ActorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
