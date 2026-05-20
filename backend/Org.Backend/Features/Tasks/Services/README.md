# Tasks Services

## ITaskService / TaskService
**Methods**:
- `Task<TaskDto> CreateTaskAsync(Guid categoryId, CreateTaskRequest request, Guid userId)`
- `Task<TaskDto> GetTaskAsync(Guid taskId, Guid userId)`
- `Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, Guid userId)`
- `Task DeleteTaskAsync(Guid taskId, Guid userId)`
- `Task<TaskDto> UpdateTaskStatusAsync(Guid taskId, TaskStatus newStatus, Guid userId)`
- `Task<TaskDto> AssignTaskAsync(Guid taskId, Guid assigneeId, Guid userId)`

## NOT Implemented in Phase 3C
- ❌ No real service implementations
