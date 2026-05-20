using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class EventRatingConfiguration : IEntityTypeConfiguration<EventRating>
{
    public void Configure(EntityTypeBuilder<EventRating> builder)
    {
        builder.ToTable("EventRatings");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Rating)
            .IsRequired();

        builder.Property(e => e.Aspect)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Comment)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(e => e.EventId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.EventId, e.UserId, e.Aspect });

        // Relationships
        builder.HasOne(e => e.Event)
            .WithMany(e => e.EventRatings)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany(e => e.EventRatings)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
