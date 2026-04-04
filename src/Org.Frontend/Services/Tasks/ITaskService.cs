using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Org.Shared.Features.Tasks;

namespace Org.Frontend.Services.Tasks
{
    public interface ITaskService
    {
        Task<List<TaskDto>> GetTasksAsync(Guid categoryId);
        Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequest req);

        Task<TaskDto> CreateTaskAsync(Guid categoryId, CreateTaskRequest req);
    }
}