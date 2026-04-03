<<<<<<< HEAD
namespace Org.Backend.Domain.Entities;

/// <summary>
/// Category node under a milestone. Supports parent-child hierarchy.
/// </summary>
public class EventCategory : BaseEntity
{
    public Guid MilestoneId { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; } = 0;

    // Navigation
    public Milestone Milestone { get; set; } = null!;
    public EventCategory? ParentCategory { get; set; }
    public ICollection<EventCategory> Children { get; set; } = [];
    public ICollection<OrgTask> Tasks { get; set; } = [];

    // TODO(BE-DAY1): Enforce max depth and sibling ordering rule at application layer.
    // TODO(BE-DAY1): Add unique constraint (MilestoneId, ParentCategoryId, CategoryName).
    // TODO(BE-DAY1): Prevent cyclical parent assignment in update endpoint.
=======
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
>>>>>>> origin/dev
}
