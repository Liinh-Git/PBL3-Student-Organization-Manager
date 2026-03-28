namespace Org.Backend.Domain.Entities;

/// <summary>
/// Represents a permission key that can be assigned to roles.
/// moduleGroup groups permissions logically (e.g., "Event", "Finance").
/// </summary>
public class Permission : BaseEntity
{
    public string PermissionKey { get; set; } = string.Empty;  // unique key, e.g. "event.create"
    public string DisplayName { get; set; } = string.Empty;
    public string? ModuleGroup { get; set; }

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
