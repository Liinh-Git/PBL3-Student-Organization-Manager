// ---- Frontend ViewModels cho Milestones ----
namespace Org.Frontend.ViewModels;

/// <summary>
/// Display model for milestone timeline and event detail pages.
/// </summary>
public sealed class MilestoneViewModel
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int OrderIndex { get; set; }
}

/// <summary>
/// Form model for milestone creation from FE dialogs.
/// </summary>
public sealed class CreateMilestoneViewModel
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int OrderIndex { get; set; }
}

/// <summary>
/// Form model for editing milestone schedule and title.
/// </summary>
public sealed class UpdateMilestoneViewModel
{
    public Guid MilestoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int OrderIndex { get; set; }
}

/// <summary>
/// View helper model for timeline rendering status and style choices.
/// </summary>
public sealed class MilestoneTimelineItemViewModel
{
    public Guid MilestoneId { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public bool IsCompleted { get; set; }
}
