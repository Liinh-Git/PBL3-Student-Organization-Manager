// ---- DTO dùng chung giữa FE layer nội bộ (mock service) ----
// Lưu ý: MemberDto tại đây có cấu trúc khác với Features/Members/MemberDto
// vì dùng RoleId (Guid) thay vì MemberRole enum — phù hợp với mock data
namespace Org.Shared.Contracts;

// ---- Thông tin thành viên (dùng cho FE mock và component nội bộ) ----
public sealed class MemberDto
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    // Lưu ý: dùng RoleId (Guid) thay vì enum MemberRole để tương thích với mock store
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime JoinDate { get; set; }
}

// ---- Yêu cầu gán vai trò cho thành viên (FE nội bộ) ----
public sealed class AssignRoleRequest
{
    public Guid RoleId { get; set; }
}

// ---- Yêu cầu gán phòng ban cho thành viên (FE nội bộ) ----
public sealed class AssignDepartmentRequest
{
    public Guid DepartmentId { get; set; }
}
