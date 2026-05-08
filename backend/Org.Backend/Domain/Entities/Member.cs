using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Membership của user trong organization, là điểm nối cho org access, department và role.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class Member : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid OrgId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? RoleId { get; set; }
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public MemberStatus Status { get; set; } = MemberStatus.Active;
    public string? StudentCode { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual Department? Department { get; set; }
    public virtual Role? Role { get; set; }
    public virtual ICollection<Department> ManagedDepartments { get; set; } = new List<Department>();
    public virtual ICollection<OrgTask> AssignedTasks { get; set; } = new List<OrgTask>();
    public virtual ICollection<OrgTask> CreatedTasks { get; set; } = new List<OrgTask>();
    public virtual ICollection<EventMember> EventMemberships { get; set; } = new List<EventMember>();
    public virtual ICollection<Request> ReviewedRequests { get; set; } = new List<Request>();
    public virtual ICollection<EventReport> EventReports { get; set; } = new List<EventReport>();
}
