namespace Org.Backend.Domain.Entities;

/// <summary>
/// Role tùy chỉnh trong organization.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class Role : BaseEntity
{
    public Guid OrgId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
    public int? Level { get; set; }

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
