// ---- ViewModels dùng riêng cho frontend (UI display) — tách biệt với API contracts ----
namespace Org.Frontend.ViewModels;

/// <summary>
/// ViewModel hiển thị event trong list card — có các field UI-only không thuộc backend contract.
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
/// ViewModel tạo event mới từ form UI.
/// </summary>
public sealed class CreateEventViewModel
{
    public string Title { get; set; } = string.Empty;
    public DateTime? Date { get; set; } = DateTime.Today;
    public TimeSpan? Time { get; set; } = new TimeSpan(8, 0, 0);
    public string? Location { get; set; }
    public int TotalSlots { get; set; }
}
