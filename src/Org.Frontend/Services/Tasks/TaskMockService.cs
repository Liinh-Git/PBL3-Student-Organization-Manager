// ---- TaskMockService ----
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Tasks;

public class TaskMockService : ITaskService
{
    // Static để data không bị reset khi chuyển trang
    private static List<TaskViewModel> _tasks = new()
    {
        new() { Id = Guid.NewGuid(), CategoryId = Guid.Empty, Title = "Lên kịch bản MC",                Status = "TODO",        AssigneeName = "Sarah J.", DueDate = DateTime.Today.AddDays(2) },
        new() { Id = Guid.NewGuid(), CategoryId = Guid.Empty, Title = "Thiết kế Backdrop",              Status = "TODO",        AssigneeName = "Marcus V.", DueDate = DateTime.Today.AddDays(3) },
        new() { Id = Guid.NewGuid(), CategoryId = Guid.Empty, Title = "Thuê dàn âm thanh ánh sáng",    Status = "IN_PROGRESS", AssigneeName = "David C.",  DueDate = DateTime.Today.AddDays(1) },
        new() { Id = Guid.NewGuid(), CategoryId = Guid.Empty, Title = "Khảo sát hội trường",           Status = "DONE",        AssigneeName = "David C.",  DueDate = DateTime.Today.AddDays(-1) }
    };

    public Task<List<TaskViewModel>> GetTasksAsync(Guid categoryId)
        => Task.FromResult(_tasks.ToList());

    public Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusViewModel req)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null) task.Status = req.Status;
        return Task.CompletedTask;
    }

    public Task<TaskViewModel> CreateTaskAsync(Guid categoryId, CreateTaskViewModel req)
    {
        var newTask = new TaskViewModel
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Title = req.Title ?? "New Task",
            Status = "TODO",
            AssigneeName = "Chưa gán",
            DueDate = req.DueDate
        };
        _tasks.Insert(0, newTask);
        return Task.FromResult(newTask);
    }
}