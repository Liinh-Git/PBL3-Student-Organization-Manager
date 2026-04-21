// ---- Thành viên của một tổ chức — liên kết giữa User và Organization ----
namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho tư cách thành viên của một User trong một Organization.
/// - Một User có thể là thành viên của nhiều tổ chức (composite unique index UserId+OrgId).
/// - Role: vai trò trong tổ chức (có thể null nếu chưa gán).
/// - Department: phòng ban đang thuộc (có thể null nếu chưa phân công).
/// - IsDeleted = true: thành viên đã bị xóa mềm (vẫn giữ lịch sử tham gia).
/// </summary>
public class Member : BaseEntity
{
    // FK → User (người dùng hệ thống)
    public Guid UserId { get; set; }
    // FK → Organization (tổ chức mà user tham gia)
    public Guid OrgId { get; set; }
    // FK → Department (phòng ban, null nếu chưa phân công)
    public Guid? DepartmentId { get; set; }
    // FK → Role (vai trò trong tổ chức, null nếu chưa gán)
    public Guid? RoleId { get; set; }
    // Ngày gia nhập tổ chức
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public Department? Department { get; set; }
    public Role? Role { get; set; }
    public ICollection<EventMember> EventMembers { get; set; } = [];
    public ICollection<OrgTask> AssignedTasks { get; set; } = [];
    public ICollection<DigitalAsset> UploadedAssets { get; set; } = [];
}
