namespace Org.Shared.Features.Tasks;

/// <summary>
/// Request DTO for creating a task
/// </summary>
public record CreateTaskRequest
{
    public required string TaskName { get; init; }
    public string? Description { get; init; }
    public Guid? AssigneeId { get; init; }
    public Guid? DeptId { get; init; }
    public DateTime? Deadline { get; init; }
    public string? Priority { get; init; } // Low, Medium, High, Urgent
    public int? OrderIndex { get; init; }
    public string? Note { get; init; }
}

/// <summary>
/// Request DTO for updating a task
/// </summary>
public record UpdateTaskRequest
{
    public required string TaskName { get; init; }
    public string? Description { get; init; }
    public Guid? AssigneeId { get; init; }
    public Guid? DeptId { get; init; }
    public DateTime? Deadline { get; init; }
    public string? Priority { get; init; } // Low, Medium, High, Urgent
    public string? Status { get; init; } // Todo, InProgress, Blocked, Done, Cancelled
    public int? OrderIndex { get; init; }
    public string? Note { get; init; }
}

/// <summary>
/// Request DTO for updating task status
/// </summary>
public record UpdateTaskStatusRequest
{
    public required string Status { get; init; } // Todo, InProgress, Blocked, Done, Cancelled
}

/// <summary>
/// Request DTO for assigning a task
/// </summary>
public record AssignTaskRequest
{
    public Guid? AssigneeId { get; init; }
    public Guid? DeptId { get; init; }
}

/// <summary>
/// Response DTO for task
/// </summary>
public record TaskDto
{
    public required Guid Id { get; init; }
    public required Guid EventCategoryId { get; init; }
    public required string TaskName { get; init; }
    public string? Description { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? AssigneeName { get; init; }
    public Guid? DeptId { get; init; }
    public string? DeptName { get; init; }
    public Guid? CreatedByMemberId { get; init; }
    public string? CreatedByMemberName { get; init; }
    public DateTime? Deadline { get; init; }
    public required string Priority { get; init; }
    public required string Status { get; init; }
    public int? OrderIndex { get; init; }
    public string? Note { get; init; }
    public DateTime? CompletedAt { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
