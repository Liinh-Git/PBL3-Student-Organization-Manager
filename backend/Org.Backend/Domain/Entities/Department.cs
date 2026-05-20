using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Phòng ban/ban trong organization, có manager và phân công task.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class Department : BaseEntity
{
    public Guid OrgId { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Function { get; set; }
    public Guid? ManagerId { get; set; }
    public DepartmentStatus Status { get; set; } = DepartmentStatus.Active;

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Member? Manager { get; set; }
    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
    public virtual ICollection<EventCategory> OwnedCategories { get; set; } = new List<EventCategory>();
    public virtual ICollection<OrgTask> AssignedTasks { get; set; } = new List<OrgTask>();
}
