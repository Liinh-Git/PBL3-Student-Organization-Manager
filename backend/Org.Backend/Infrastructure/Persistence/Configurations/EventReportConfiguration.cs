using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class EventReportConfiguration : IEntityTypeConfiguration<EventReport>
{
    public void Configure(EntityTypeBuilder<EventReport> builder)
    {
        builder.ToTable("EventReports");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.ActualAttendance)
            .IsRequired(false);

        builder.Property(e => e.ActualBudget)
            .HasColumnType("numeric(18,2)")
            .IsRequired(false);

        builder.Property(e => e.RatingAverage)
            .IsRequired(false);

        builder.Property(e => e.Summary)
            .HasColumnType("text");

        builder.Property(e => e.CreatedByMemberId)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(e => e.EventId)
            .IsUnique();

        builder.HasIndex(e => e.CreatedByMemberId);

        // Relationships
        builder.HasOne(e => e.Event)
            .WithOne(e => e.EventReport)
            .HasForeignKey<EventReport>(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreatedByMember)
            .WithMany(e => e.EventReports)
            .HasForeignKey(e => e.CreatedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
