// ---- Phòng ban trong một tổ chức ----
namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho một phòng ban (đơn vị con) trong tổ chức.
/// - Code: mã viết tắt tự sinh từ tên nếu không cung cấp (vd: "Kỹ Thuật" → "KYTHUAT").
/// - ManagerId: FK đến Member (không phải User) — người quản lý phải là thành viên của tổ chức.
///   Dùng OnDelete SetNull để xóa member không kéo theo xóa phòng ban.
/// - Function: mô tả ngắn về chức năng phòng ban.
/// </summary>
public class Department : BaseEntity
{
    // FK → Organization
    public Guid OrgId { get; set; }
    // Tên phòng ban hiển thị
    public string DeptName { get; set; } = string.Empty;
    // Mã viết tắt tự sinh nếu để trống (≤8 ký tự chữ số, viết hoa)
    public string? Code { get; set; }
    // FK → Member (trưởng phòng), null nếu chưa bổ nhiệm
    public Guid? ManagerId { get; set; }
    // Mô tả chức năng / nhiệm vụ của phòng ban
    public string? Function { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Member? Manager { get; set; }
    public ICollection<Member> Members { get; set; } = [];
    public ICollection<EventCategory> OwnedEventCategories { get; set; } = [];
    public ICollection<OrgTask> Tasks { get; set; } = [];
}
