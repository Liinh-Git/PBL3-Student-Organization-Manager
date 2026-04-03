// ---- Nhóm công việc trong một milestone (ví dụ: hậu cần, kỹ thuật) ----
namespace Org.Backend.Domain.Entities;

/// <summary>
/// A workstream category within a milestone (e.g., logistics, technical).
/// Tasks are attached to categories rather than directly to milestones.
/// </summary>
public class EventCategory : BaseEntity
{
    // FK -> Milestone
    public Guid MilestoneId { get; set; }
    // Tên nhóm công việc hiển thị trên board
    public string CategoryName { get; set; } = string.Empty;
    // Thứ tự hiển thị trong milestone
    public int OrderIndex { get; set; } = 0;
    // Mô tả ngắn cho nhóm công việc
    public string? Description { get; set; }
    // FK -> Department phụ trách category
    public Guid? OwnerDepartmentId { get; set; }

    // Navigation
    public Milestone Milestone { get; set; } = null!;
    public Department? OwnerDepartment { get; set; }
    public ICollection<OrgTask> Tasks { get; set; } = [];
}
