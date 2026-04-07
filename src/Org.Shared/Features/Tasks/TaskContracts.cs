namespace Org.Shared.Features.Tasks;

public sealed record TaskDto(
    Guid Id,
    Guid CategoryId,
    Guid? AssigneeMemberId,
    string Title,
    string? Description,
    TaskStatus Status,
    DateOnly? DueDate,
    TaskPriority Priority,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record CreateTaskRequest(
    Guid CategoryId,
    string Title,
    string? Description,
    Guid? AssigneeMemberId,
    DateOnly? DueDate,
    TaskPriority Priority);

public sealed record GetTasksResponse(IReadOnlyList<TaskDto> Items);

public sealed record UpdateTaskStatusRequest(TaskStatus Status);

public sealed record AssignTaskRequest(Guid? AssigneeMemberId);
