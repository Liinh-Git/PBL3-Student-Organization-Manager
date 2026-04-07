// ---- Frontend ViewModels cho Milestones ----
namespace Org.Frontend.ViewModels;

public sealed class MilestoneViewModel
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int OrderIndex { get; set; }
}
