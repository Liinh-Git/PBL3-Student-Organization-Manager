namespace Org.Shared.Features.Tasks;

public enum OrgTaskStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2,
    Blocked = 3
}

public sealed record TaskDto(
    Guid Id,
    Guid CategoryId,
    Guid? AssigneeMemberId,
    string Title,
    string? Description,
    OrgTaskStatus Status,
    DateOnly? DueDate,
    int Priority,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record CreateTaskRequest(
    Guid CategoryId,
    string Title,
    string? Description,
    Guid? AssigneeMemberId,
    DateOnly? DueDate,
    int Priority);

public sealed record GetTasksResponse(IReadOnlyList<TaskDto> Items);

public sealed record UpdateTaskStatusRequest(OrgTaskStatus Status);

public sealed record AssignTaskRequest(Guid? AssigneeMemberId);
