// ---- DTO và request dùng chung giữa FE và BE cho module nhiệm vụ (task) ----
namespace Org.Shared.Features.Tasks;

// ---- Thông tin đầy đủ một nhiệm vụ ----
// CategoryId: FK đến EventCategory — mỗi task thuộc về một hạng mục của cột mốc sự kiện
// Description: ghi chú / mô tả công việc (alias cho Note trong entity OrgTask)
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

// ---- Yêu cầu tạo nhiệm vụ mới trong một hạng mục ----
public sealed record CreateTaskRequest(
    Guid CategoryId,
    string Title,
    string? Description,
    Guid? AssigneeMemberId,
    DateOnly? DueDate,
    TaskPriority Priority);

// ---- Phản hồi danh sách nhiệm vụ ----
public sealed record GetTasksResponse(IReadOnlyList<TaskDto> Items);

// ---- Phản hồi chi tiết một nhiệm vụ ----
public sealed record GetTaskByIdResponse(TaskDto Data);

// ---- Yêu cầu cập nhật toàn bộ thông tin nhiệm vụ (trừ status) ----
public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    Guid? AssigneeMemberId,
    DateOnly? DueDate,
    TaskPriority Priority);

// ---- Yêu cầu thay đổi trạng thái nhiệm vụ (Todo → InProgress → Done) ----
public sealed record UpdateTaskStatusRequest(TaskStatus Status);

// ---- Yêu cầu gán / bỏ gán người thực hiện (null = bỏ gán) ----
public sealed record AssignTaskRequest(Guid? AssigneeMemberId);
