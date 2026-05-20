using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.OrgName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(e => e.CoverUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Location)
            .HasMaxLength(500);

        builder.Property(e => e.ContactEmail)
            .HasMaxLength(256);

        builder.Property(e => e.ContactPhone)
            .HasMaxLength(20);

        builder.Property(e => e.TotalMembers)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        // Indexes
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.OrgName);

        // Relationships
        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Members)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Departments)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Roles)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Events)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Requests)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Resources)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ActivityHistories)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
