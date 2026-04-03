using Org.Backend.Domain.Enums;
using TaskStatus = Org.Backend.Domain.Enums.TaskStatus;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// A task belonging to an event category.
/// Renamed OrgTask to avoid conflict with System.Threading.Tasks.Task.
/// Deadline must be before the due date of the milestone associated with the parent event category.
/// </summary>
public class OrgTask : BaseEntity
{
    public Guid EventCategoryId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public Guid? AssigneeId { get; set; }   // FK → Member
    public Guid? DeptId { get; set; }        // FK → Department
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? Deadline { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public string? Note { get; set; }

    // Navigation
    public EventCategory EventCategory { get; set; } = null!;
    public Member? Assignee { get; set; }
    public Department? Department { get; set; }
}
