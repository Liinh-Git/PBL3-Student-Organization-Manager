using Org.Backend.Domain.Enums;
using TaskStatus = Org.Backend.Domain.Enums.TaskStatus;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// A task belonging to a milestone.
/// Renamed OrgTask to avoid conflict with System.Threading.Tasks.Task.
/// deadline must be before the parent Milestone.dueDate (enforced by application logic).
/// </summary>
public class OrgTask : BaseEntity
{
    public Guid MilestoneId { get; set; }
    public Guid? CategoryId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public Guid? AssigneeId { get; set; }   // FK → Member
    public Guid? DeptId { get; set; }        // FK → Department
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? Deadline { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public string? Note { get; set; }

    // Navigation
    public Milestone Milestone { get; set; } = null!;
    public EventCategory? Category { get; set; }
    public Member? Assignee { get; set; }
    public Department? Department { get; set; }

    // TODO(BE-DAY1): Make CategoryId required after migration is completed.
    // TODO(BE-DAY1): Add status transition policy (Todo -> InProgress -> Done/Blocked).
}
