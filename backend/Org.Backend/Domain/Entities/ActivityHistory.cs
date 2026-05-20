using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Feed log hoạt động của organization.
/// Scope: SHOULD_HAVE_DB_V1_NO_WORKING_UI_YET.
/// UI/API working: no.
/// </summary>
public class ActivityHistory : BaseEntity
{
    public Guid OrgId { get; set; }
    public string Title { get; set; } = string.Empty;
    public ActivityType Type { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public bool IsPublic { get; set; } = false;

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
}
