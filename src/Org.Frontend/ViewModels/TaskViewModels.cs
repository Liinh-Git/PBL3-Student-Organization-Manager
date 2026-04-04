// ---- Frontend ViewModels cho Tasks ----
namespace Org.Frontend.ViewModels;

public sealed class TaskViewModel
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "TODO";   // "TODO" | "IN_PROGRESS" | "DONE"
    public string? AssigneeName { get; set; }       // UI-only: tên hiển thị
    public DateTime? DueDate { get; set; }
}

public sealed class CreateTaskViewModel
{
    public string? Title { get; set; }
    public DateTime? DueDate { get; set; } = DateTime.Today.AddDays(1);
}

public sealed class UpdateTaskStatusViewModel
{
    public string Status { get; set; } = "TODO";
}
