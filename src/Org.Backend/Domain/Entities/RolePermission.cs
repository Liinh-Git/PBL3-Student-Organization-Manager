namespace Org.Backend.Domain.Entities;

/// <summary>
/// Join table between Role and Permission (many-to-many).
/// No surrogate PK — composite PK (RoleId, PermissionId) configured in AppDbContext.
/// Does NOT inherit BaseEntity (no Id / CreatedAt needed for a pure join table).
/// </summary>
public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    // Navigation
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
