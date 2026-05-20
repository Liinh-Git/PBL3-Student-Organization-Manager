namespace Org.Backend.Domain.Entities;

/// <summary>
/// Permission key cho authorization theo module.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class Permission : BaseEntity
{
    public string PermissionKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ModuleGroup { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
