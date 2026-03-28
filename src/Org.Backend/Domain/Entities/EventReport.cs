namespace Org.Backend.Domain.Entities;

/// <summary>
/// Post-event report: attendance, budget actuals, rating summary.
/// One-to-one with Event.
/// </summary>
public class EventReport : BaseEntity
{
    public Guid EventId { get; set; }
    public int ActualAttendance { get; set; } = 0;
    public decimal ActualBudget { get; set; } = 0;
    public float RatingAverage { get; set; } = 0;
    public string? Summary { get; set; }  // long text

    // Navigation
    public Event Event { get; set; } = null!;
}
