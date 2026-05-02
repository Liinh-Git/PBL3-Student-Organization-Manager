// ---- ViewModels dùng riêng cho frontend (UI display) — tách biệt với API contracts ----
namespace Org.Frontend.ViewModels;

using System;
using System.Collections.Generic;

/// <summary>
/// Display model used by EventList and EventDetail pages.
/// It intentionally contains UI-only fields that do not exist in API contracts.
/// </summary>
public sealed class EventViewModel
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StatusLabel { get; set; } = string.Empty;   // "Ongoing", "Upcoming", "Completed", "Draft"
    public string? Location { get; set; }                     // UI-only field
    public int RegisteredCount { get; set; }                  // UI-only
    public int ParticipantCount => RegisteredCount;           // Alias for UI consistency
    public int TotalSlots { get; set; }                       // UI-only
    public string? ImageUrl { get; set; }                     // UI-only
    
    // Dashboard stats — chỉ dùng ở EventDetail, populate từ mock/API sau
    public string? CompletionLabel { get; set; }              // "75%"
    public double CompletionPercentage { get; set; }          // 75.0
    public string? BudgetLabel { get; set; }                  // "82%"
    public string? RiskLevel { get; set; }                    // "Low", "Medium", "High"
    public int TotalFiles { get; set; }
    public decimal ActualSpending { get; set; }
    public bool CanManage { get; set; }
    public bool CanEnterWorkspace { get; set; }
}

public sealed class MyEventsViewModel
{
    public List<MyEventItemViewModel> OrganizerEvents { get; set; } = [];
    public List<MyEventItemViewModel> AttendeeEvents { get; set; } = [];
}

public sealed class MyEventItemViewModel
{
    public Guid EventId { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StatusLabel { get; set; } = "UPCOMING";
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsOrganizer { get; set; }
    public bool CanEnterWorkspace { get; set; }
    public bool CanManage { get; set; }
}

/// <summary>
/// Form input model for creating a new event from FE dialog.
/// (Đã bổ sung EndDate, EndTime, Tags và đổi TotalSlots thành ExpectedParticipants)
/// </summary>
public sealed class CreateEventViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public DateTime? StartDate { get; set; } = DateTime.Today;
    public TimeSpan? StartTime { get; set; } = new TimeSpan(8, 0, 0);
    
    public DateTime? EndDate { get; set; } = DateTime.Today;
    public TimeSpan? EndTime { get; set; } = new TimeSpan(17, 0, 0);
    
    public string? Location { get; set; }
    public int ExpectedParticipants { get; set; }
    
    public List<string> Tags { get; set; } = new List<string>();
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
