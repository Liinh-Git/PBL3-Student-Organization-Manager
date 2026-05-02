// ---- Thực thể tổ chức (câu lạc bộ, nhóm, ...) ----
using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho một tổ chức trong hệ thống (câu lạc bộ, nhóm sinh viên, ...).
/// - OrgName: tên tổ chức, unique trên toàn hệ thống.
/// - TotalMembers: số đếm được cập nhật qua application logic (không tự động qua trigger).
/// - Status: Active/Inactive — dùng để ẩn tổ chức mà không xóa dữ liệu.
/// </summary>
public class Organization : BaseEntity
{
    public string OrgName { get; set; } = string.Empty;
    public string? Description { get; set; }
    // Đường dẫn ảnh đại diện (có thể null)
    public string? AvatarUrl { get; set; }
    // Đường dẫn ảnh bìa (có thể null)
    public string? CoverUrl { get; set; }
    public DateTime? FoundingDate { get; set; }
    public string? Location { get; set; }
    // Số thành viên active — được cập nhật thủ công qua application logic
    public int TotalMembers { get; set; } = 0;
    public OrgStatus Status { get; set; } = OrgStatus.Active;

    // Navigation
    public ICollection<Department> Departments { get; set; } = [];
    public ICollection<Member> Members { get; set; } = [];
    public ICollection<Event> Events { get; set; } = [];
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<Resource> Resources { get; set; } = [];
    public ICollection<Request> Requests { get; set; } = [];
    public ICollection<ActivityHistory> ActivityHistories { get; set; } = [];
    public ICollection<OrganizationPost> Posts { get; set; } = [];
}
