namespace Org.Backend.Domain.Entities;

/// <summary>
/// Hạng mục trong milestone để chứa task.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class EventCategory : BaseEntity
{
    public Guid MilestoneId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public Guid? OwnerDepartmentId { get; set; }

    // Navigation properties
    public virtual Milestone Milestone { get; set; } = null!;
    public virtual Department? OwnerDepartment { get; set; }
    public virtual ICollection<OrgTask> Tasks { get; set; } = new List<OrgTask>();
}
