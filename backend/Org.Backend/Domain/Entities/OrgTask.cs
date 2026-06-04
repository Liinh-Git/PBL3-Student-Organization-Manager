using Org.Backend.Domain.Enums;
using DomainTaskStatus = Org.Backend.Domain.Enums.TaskStatus;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Task của một hạng mục event.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class OrgTask : BaseEntity
{
    public Guid? EventCategoryId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid? DeptId { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? Deadline { get; set; }
    public DomainTaskStatus Status { get; set; } = DomainTaskStatus.Todo;
    public string? Note { get; set; }
    public Guid? CreatedByMemberId { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual EventCategory? EventCategory { get; set; }
    public virtual Member? Assignee { get; set; }
    public virtual Department? Department { get; set; }
    public virtual Member? CreatedByMember { get; set; }
}
