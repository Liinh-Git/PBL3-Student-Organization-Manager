using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Aggregate gốc của workspace tổ chức.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class Organization : BaseEntity
{
    public string OrgName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime? FoundingDate { get; set; }
    public string? Location { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int TotalMembers { get; set; } = 0;
    public OrgStatus Status { get; set; } = OrgStatus.Active;
    public Guid? CreatedByUserId { get; set; }

    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
    public virtual ICollection<ActivityHistory> ActivityHistories { get; set; } = new List<ActivityHistory>();
}
