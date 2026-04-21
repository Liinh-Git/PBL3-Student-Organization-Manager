// ---- Frontend ViewModels cho EventCategories ----
namespace Org.Frontend.ViewModels;

/// <summary>
/// Display model for milestone category table and workflow board rows.
/// </summary>
public sealed class EventCategoryViewModel
{
    public Guid Id { get; set; }
    public Guid MilestoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    // UI-only fields cho mock hiển thị trưởng ban và tiến độ
    public string? LeadName { get; set; }
    public string? LeadAvatarUrl { get; set; }
    public int ActiveSubtasks { get; set; }
    public int ProgressPercentage { get; set; }
}

/// <summary>
/// Form model for creating category inside a milestone.
/// </summary>
public sealed class CreateEventCategoryViewModel
{
    public Guid MilestoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? LeadMemberId { get; set; }
}

/// <summary>
/// Form model for editing category metadata.
/// </summary>
public sealed class UpdateEventCategoryViewModel
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? LeadMemberId { get; set; }
}

/// <summary>
/// Summary block for category-level task metrics in dashboard cards.
/// </summary>
public sealed class EventCategorySummaryViewModel
{
    public Guid CategoryId { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int ActiveTasks { get; set; }
}
