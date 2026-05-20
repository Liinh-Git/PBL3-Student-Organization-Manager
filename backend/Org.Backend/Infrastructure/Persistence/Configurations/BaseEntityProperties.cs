using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

/// <summary>
/// Helper methods for configuring BaseEntity properties on derived entity types.
/// </summary>
public static class BaseEntityProperties
{
    /// <summary>
    /// Configures the common properties from BaseEntity on a derived entity type.
    /// Call this method in each entity configuration that inherits from BaseEntity.
    /// </summary>
    public static void ConfigureBaseEntityProperties<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now() AT TIME ZONE 'UTC'");

        builder.Property(e => e.UpdatedAt)
            .IsRequired(false);

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedAt)
            .IsRequired(false);
    }
}
