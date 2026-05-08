using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Rating của user cho event, hỗ trợ cache AverageRating.
/// Scope: SHOULD_HAVE_DB_V1_NO_WORKING_UI_YET.
/// UI/API working: no.
/// </summary>
public class EventRating : BaseEntity
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public RatingAspect Aspect { get; set; }
    public string? Comment { get; set; }

    // Navigation properties
    public virtual Event Event { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
