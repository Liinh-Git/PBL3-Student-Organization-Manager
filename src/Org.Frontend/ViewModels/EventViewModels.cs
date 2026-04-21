// ---- ViewModels dùng riêng cho frontend (UI display) — tách biệt với API contracts ----
namespace Org.Frontend.ViewModels;

/// <summary>
/// Display model used by EventList and EventDetail pages.
/// It intentionally contains UI-only fields that do not exist in API contracts.
/// </summary>
public sealed class EventViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StatusLabel { get; set; } = string.Empty;   // "Ongoing", "Upcoming", "Completed", "Draft"
    public string? Location { get; set; }                     // UI-only field
    public int RegisteredCount { get; set; }                  // UI-only
    public int TotalSlots { get; set; }                       // UI-only
    public string? ImageUrl { get; set; }                     // UI-only
    // Dashboard stats — chỉ dùng ở EventDetail, populate từ mock/API sau
    public string? CompletionLabel { get; set; }              // "75%"
    public string? BudgetLabel { get; set; }                  // "82%"
    public string? RiskLevel { get; set; }                    // "Low", "Medium", "High"
    public int TotalFiles { get; set; }
    public decimal ActualSpending { get; set; }
}

/// <summary>
/// Form input model for creating a new event from FE dialog.
/// </summary>
public sealed class CreateEventViewModel
{
    public string Title { get; set; } = string.Empty;
    public DateTime? Date { get; set; } = DateTime.Today;
    public TimeSpan? Time { get; set; } = new TimeSpan(8, 0, 0);
    public string? Location { get; set; }
    public int TotalSlots { get; set; }
}

/// <summary>
/// Form input model for updating existing event metadata in FE workflows.
/// This is separate from CreateEventViewModel so edit validations can evolve independently.
/// </summary>
public sealed class UpdateEventViewModel
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public int? TotalSlots { get; set; }
}

/// <summary>
/// UI state model for list filtering and quick search in EventList page.
/// </summary>
public sealed class EventListFilterViewModel
{
    public string? SearchTerm { get; set; }
    public string Status { get; set; } = "ALL";
    public DateTime? StartFrom { get; set; }
    public DateTime? EndTo { get; set; }
}

/// <summary>
/// Snapshot block for dashboard-style metrics displayed on event detail screen.
/// </summary>
public sealed class EventDashboardMetricsViewModel
{
    public string CompletionLabel { get; set; } = "0%";
    public string BudgetLabel { get; set; } = "0%";
    public string RiskLevel { get; set; } = "Low";
    public int TotalFiles { get; set; }
    public decimal ActualSpending { get; set; }
}
