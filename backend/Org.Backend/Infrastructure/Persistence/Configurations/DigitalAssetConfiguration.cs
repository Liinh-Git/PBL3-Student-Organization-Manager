using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class DigitalAssetConfiguration : IEntityTypeConfiguration<DigitalAsset>
{
    public void Configure(EntityTypeBuilder<DigitalAsset> builder)
    {
        builder.ToTable("DigitalAssets");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.FileUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.FileType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.UploadedByUserId)
            .IsRequired(false);

        builder.Property(e => e.UploadedAt)
            .IsRequired()
            .HasDefaultValueSql("now() AT TIME ZONE 'UTC'");

        // Indexes
        builder.HasIndex(e => e.EventId);
        builder.HasIndex(e => e.UploadedByUserId);
        builder.HasIndex(e => e.FileType);

        // Relationships
        builder.HasOne(e => e.Event)
            .WithMany(e => e.DigitalAssets)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.UploadedByUser)
            .WithMany(e => e.UploadedDigitalAssets)
            .HasForeignKey(e => e.UploadedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
