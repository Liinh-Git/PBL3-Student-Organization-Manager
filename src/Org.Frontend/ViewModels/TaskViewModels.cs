// ---- Frontend ViewModels cho Tasks ----
namespace Org.Frontend.ViewModels;

/// <summary>
/// Display model used by list and Kanban task board rendering.
/// </summary>
public sealed class TaskViewModel
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "TODO";   // "TODO" | "IN_PROGRESS" | "DONE"
    public string? AssigneeName { get; set; }       // UI-only: tên hiển thị
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Form input model for creating a task from FE dialog.
/// </summary>
public sealed class CreateTaskViewModel
{
    public string? Title { get; set; }
    public DateTime? DueDate { get; set; } = DateTime.Today.AddDays(1);
}

/// <summary>
/// Action model used by drag/drop and status action updates.
/// </summary>
public sealed class UpdateTaskStatusViewModel
{
    public string Status { get; set; } = "TODO";
}

/// <summary>
/// Form model for future task edit flows.
/// Keeps update intent explicit instead of reusing create model.
/// </summary>
public sealed class UpdateTaskViewModel
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "TODO";
}

/// <summary>
/// Kanban column projection model to group tasks by status.
/// </summary>
public sealed class TaskColumnViewModel
{
    public string Status { get; set; } = "TODO";
    public string Label { get; set; } = "To Do";
    public List<TaskViewModel> Items { get; set; } = [];
}

/// <summary>
/// Local filter model for task board searching and deadline windows.
/// </summary>
public sealed class TaskFilterViewModel
{
    public string? SearchTerm { get; set; }
    public string Status { get; set; } = "ALL";
    public DateTime? DueFrom { get; set; }
    public DateTime? DueTo { get; set; }
}

/// <summary>
/// Overview payload for department-level task board snippets.
/// Mirrors backend department tasks overview endpoint for FE rendering.
/// </summary>
public sealed class DepartmentTasksOverviewViewModel
{
    public Guid DepartmentId { get; set; }
    public int TotalTasks { get; set; }
    public int OpenTaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public List<DepartmentTaskItemViewModel> Items { get; set; } = [];
}

/// <summary>
/// Single task row inside department overview payload.
/// </summary>
public sealed class DepartmentTaskItemViewModel
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "TODO";
    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }
    public string? AssigneeName { get; set; }
}
