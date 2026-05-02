namespace Org.Frontend.ViewModels;

public sealed class TaskViewModel
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "TODO";
    public string? Priority { get; set; } = "MEDIUM";
    public Guid? AssigneeMemberId { get; set; }
    public List<Guid> CoAssigneeMemberIds { get; set; } = [];
    public string? AssigneeName { get; set; }
    public DateTime? DueDate { get; set; }
    public bool CanMarkCompleted { get; set; }
}

public sealed class CreateTaskViewModel
{
    public string? Title { get; set; }
    public string? Priority { get; set; } = "MEDIUM";
    public List<Guid> AssigneeMemberIds { get; set; } = [];
    public DateTime? DueDate { get; set; } = DateTime.Today.AddDays(1);
}

public sealed class UpdateTaskStatusViewModel
{
    public string Status { get; set; } = "TODO";
}

public sealed class UpdateTaskViewModel
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "TODO";
}

public sealed class TaskColumnViewModel
{
    public string Status { get; set; } = "TODO";
    public string Label { get; set; } = "To Do";
    public List<TaskViewModel> Items { get; set; } = [];
}

public sealed class TaskFilterViewModel
{
    public string? SearchTerm { get; set; }
    public string Status { get; set; } = "ALL";
    public DateTime? DueFrom { get; set; }
    public DateTime? DueTo { get; set; }
}

public sealed class DepartmentTasksOverviewViewModel
{
    public Guid DepartmentId { get; set; }
    public int TotalTasks { get; set; }
    public int OpenTaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public List<DepartmentTaskItemViewModel> Items { get; set; } = [];
}

public sealed class DepartmentTaskItemViewModel
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "TODO";
    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }
    public string? AssigneeName { get; set; }
}
