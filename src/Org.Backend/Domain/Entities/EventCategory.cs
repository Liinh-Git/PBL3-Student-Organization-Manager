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
}
