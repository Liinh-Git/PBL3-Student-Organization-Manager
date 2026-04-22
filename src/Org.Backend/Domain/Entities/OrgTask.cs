// ---- Nhiệm vụ trong một hạng mục sự kiện ----
using Org.Backend.Domain.Enums;
using Org.Shared;
using TaskStatus = Org.Shared.TaskStatus;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho một nhiệm vụ (task) thuộc về một EventCategory.
/// - Đổi tên thành OrgTask để tránh conflict với System.Threading.Tasks.Task.
/// - TaskName trong entity ánh xạ thành Title trong DTO (ContractMapping).
/// - Note trong entity ánh xạ thành Description trong DTO.
/// - AssigneeId: FK đến Member được giao việc (null = chưa giao).
/// - DeptId: FK đến Department phụ trách (null = không giới hạn phòng ban).
/// - Deadline nên nằm trước EndDate của Milestone cha (không enforce ở DB level).
/// </summary>
public class OrgTask : BaseEntity
{
    // FK → EventCategory (hạng mục chứa task này)
    public Guid EventCategoryId { get; set; }
    // Tên nhiệm vụ — ánh xạ thành "Title" trong DTO
    public string TaskName { get; set; } = string.Empty;
    // FK → Member (người được giao nhiệm vụ), null = chưa giao
    public Guid? AssigneeId { get; set; }
    // FK → Department (phòng ban phụ trách task), null = không giới hạn
    public Guid? DeptId { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? Deadline { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    // Ghi chú / mô tả — ánh xạ thành "Description" trong DTO
    public string? Note { get; set; }

    // Navigation
    public EventCategory EventCategory { get; set; } = null!;
    public Member? Assignee { get; set; }
    public Department? Department { get; set; }
}
