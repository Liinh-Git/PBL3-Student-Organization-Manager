namespace Org.Backend.Domain.Entities;

/// <summary>
/// A role that can be assigned to members within an organization.
/// isDefault = true means this role is auto-assigned to new members.
/// </summary>
public class Role : BaseEntity
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OrgId { get; set; }
    public bool IsDefault { get; set; } = false;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public ICollection<Member> Members { get; set; } = [];
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
