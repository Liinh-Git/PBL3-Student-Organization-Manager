using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.ToTable("FriendRequests");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.SenderId)
            .IsRequired();

        builder.Property(e => e.ReceiverId)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.RespondedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(e => new { e.SenderId, e.ReceiverId })
            .IsUnique();

        builder.HasIndex(e => e.Status);

        // Relationships
        builder.HasOne(e => e.Sender)
            .WithMany(e => e.SentFriendRequests)
            .HasForeignKey(e => e.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Receiver)
            .WithMany(e => e.ReceivedFriendRequests)
            .HasForeignKey(e => e.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
