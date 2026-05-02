// ---- Interface service tasks ----
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Tasks;

public interface ITaskService
{
    Task<List<TaskViewModel>> GetTasksAsync(Guid categoryId);
    Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusViewModel req);
    Task<TaskViewModel> CreateTaskAsync(Guid categoryId, CreateTaskViewModel req);
    Task<bool> CanManageTasksAsync(Guid categoryId);
}
