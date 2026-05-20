namespace Org.Backend.Domain.Entities;

/// <summary>
/// Báo cáo tổng kết cho event.
/// Scope: SHOULD_HAVE_DB_V1_NO_WORKING_UI_YET.
/// UI/API working: no.
/// One-to-one với Event.
/// </summary>
public class EventReport : BaseEntity
{
    public Guid EventId { get; set; }
    public int? ActualAttendance { get; set; }
    public decimal? ActualBudget { get; set; }
    public double? RatingAverage { get; set; }
    public string? Summary { get; set; }
    public Guid? CreatedByMemberId { get; set; }

    // Navigation properties
    public virtual Event Event { get; set; } = null!;
    public virtual Member? CreatedByMember { get; set; }
}
