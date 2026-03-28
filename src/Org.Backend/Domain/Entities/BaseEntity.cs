namespace Org.Backend.Domain.Entities;

/// <summary>
/// Abstract base inherited by all entities.
/// Uses Guid primary keys (matching String IDs in class diagrams).
/// EF Core applies a global soft-delete query filter (!IsDeleted) automatically.
/// SaveChangesAsync override auto-sets UpdatedAt on every modification.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
