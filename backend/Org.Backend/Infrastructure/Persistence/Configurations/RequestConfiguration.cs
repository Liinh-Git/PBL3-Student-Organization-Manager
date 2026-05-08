using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("Requests");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.SenderId)
            .IsRequired();

        builder.Property(e => e.OrgId)
            .IsRequired();

        builder.Property(e => e.RequestType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Title)
            .HasMaxLength(300);

        builder.Property(e => e.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.DesiredDepartmentId)
            .IsRequired(false);

        builder.Property(e => e.DesiredPosition)
            .HasMaxLength(200);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.ReviewNote)
            .HasMaxLength(500);

        builder.Property(e => e.ReviewedByMemberId)
            .IsRequired(false);

        builder.Property(e => e.ReviewedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(e => e.OrgId);
        builder.HasIndex(e => e.SenderId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RequestType);

        // Relationships
        builder.HasOne(e => e.Sender)
            .WithMany()
            .HasForeignKey(e => e.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Organization)
            .WithMany(e => e.Requests)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DesiredDepartment)
            .WithMany()
            .HasForeignKey(e => e.DesiredDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ReviewedByMember)
            .WithMany(e => e.ReviewedRequests)
            .HasForeignKey(e => e.ReviewedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
