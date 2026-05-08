using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Tài nguyên của organization, có thể gắn với event.
/// Scope: SHOULD_HAVE_DB_V1_NO_WORKING_UI_YET.
/// UI/API working: no.
/// </summary>
public class Resource : BaseEntity
{
    public Guid OrgId { get; set; }
    public Guid? EventId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string? Type { get; set; }
    public int Quantity { get; set; }
    public ResourceStatus Status { get; set; } = ResourceStatus.Available;
    public string? Note { get; set; }

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Event? Event { get; set; }
}
