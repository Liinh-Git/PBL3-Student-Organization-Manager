// ---- Frontend ViewModels cho EventCategories ----
namespace Org.Frontend.ViewModels;

public sealed class EventCategoryViewModel
{
    public Guid Id { get; set; }
    public Guid MilestoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    // UI-only fields cho mock hiển thị trưởng ban và tiến độ
    public string? LeadName { get; set; }
    public string? LeadAvatarUrl { get; set; }
    public int ActiveSubtasks { get; set; }
    public int ProgressPercentage { get; set; }
}
