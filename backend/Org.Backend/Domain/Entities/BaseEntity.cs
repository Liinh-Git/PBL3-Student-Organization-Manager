namespace Org.Backend.Domain.Entities;

/// <summary>
/// Nền chung cho audit timestamp và soft-delete.
/// Tất cả business entity bình thường inherit từ đây.
/// RolePermission KHÔNG inherit BaseEntity.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
