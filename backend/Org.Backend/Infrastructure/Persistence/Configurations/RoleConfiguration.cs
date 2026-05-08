using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.OrgId)
            .IsRequired();

        builder.Property(e => e.RoleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.Level)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(e => new { e.OrgId, e.RoleName })
            .IsUnique();

        builder.HasIndex(e => e.OrgId);

        // Relationships
        builder.HasOne(e => e.Organization)
            .WithMany(e => e.Roles)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Members)
            .WithOne(e => e.Role)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.RolePermissions)
            .WithOne(e => e.Role)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
