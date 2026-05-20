namespace Org.Backend.Domain.Entities;

/// <summary>
/// Mapping bảng nối role và permission.
/// KHÔNG inherit BaseEntity - đây là pure join table.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
