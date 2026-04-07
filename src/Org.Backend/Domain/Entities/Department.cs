namespace Org.Backend.Domain.Entities;

/// <summary>
/// A sub-unit of an Organization (e.g., "Technical Team").
/// managerId references the Member who manages this department.
/// </summary>
public class Department : BaseEntity
{
    public Guid OrgId { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid? ManagerId { get; set; }
    public string? Function { get; set; }

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Member? Manager { get; set; }
    public ICollection<Member> Members { get; set; } = [];
    public ICollection<EventCategory> OwnedEventCategories { get; set; } = [];
    public ICollection<OrgTask> Tasks { get; set; } = [];
}
