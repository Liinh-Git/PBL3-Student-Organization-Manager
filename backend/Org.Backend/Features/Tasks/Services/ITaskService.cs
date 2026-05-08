using Org.Shared.Features.Tasks;

namespace Org.Backend.Features.Tasks.Services;

public interface ITaskService
{
    Task<TaskDto> GetTaskByIdAsync(Guid taskId, Guid userId, CancellationToken ct = default);
    Task<TaskDto> CreateTaskAsync(Guid categoryId, CreateTaskRequest request, Guid userId, CancellationToken ct = default);
    Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, Guid userId, CancellationToken ct = default);
    Task DeleteTaskAsync(Guid taskId, Guid userId, CancellationToken ct = default);
    Task<TaskDto> UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequest request, Guid userId, CancellationToken ct = default);
    Task<TaskDto> AssignTaskAsync(Guid taskId, AssignTaskRequest request, Guid userId, CancellationToken ct = default);
}
