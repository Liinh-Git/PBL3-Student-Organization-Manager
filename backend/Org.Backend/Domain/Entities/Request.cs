using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Request join organization và workflow review.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class Request : BaseEntity
{
    public Guid SenderId { get; set; }
    public Guid OrgId { get; set; }
    public RequestType RequestType { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? DesiredDepartmentId { get; set; }
    public string? DesiredPosition { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public string? ReviewNote { get; set; }
    public Guid? ReviewedByMemberId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Navigation properties
    public virtual User Sender { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual Department? DesiredDepartment { get; set; }
    public virtual Member? ReviewedByMember { get; set; }
}
