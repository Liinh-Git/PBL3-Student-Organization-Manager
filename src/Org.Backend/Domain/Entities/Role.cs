// ---- Vai trò trong một tổ chức — gắn với quyền (Permission) qua RolePermission ----
namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho một vai trò trong tổ chức (ví dụ: Chủ tịch, Phó chủ tịch, ...).
/// - Mỗi tổ chức có bộ vai trò riêng (unique index OrgId+RoleName).
/// - IsDefault = true: vai trò được tự động gán cho thành viên mới khi tham gia.
/// - RoleName phải trùng với giá trị enum MemberRole (Member/Manager/VicePresident/President)
///   để OrganizationAuthorization.ParseRole() hoạt động chính xác.
/// </summary>
public class Role : BaseEntity
{
    // Tên vai trò — phải khớp với enum MemberRole để authorization hoạt động
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    // FK → Organization
    public Guid OrgId { get; set; }
    // true = tự động gán cho thành viên mới khi gia nhập
    public bool IsDefault { get; set; } = false;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public ICollection<Member> Members { get; set; } = [];
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
