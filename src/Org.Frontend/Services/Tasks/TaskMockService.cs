using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Org.Shared.Features.Tasks;

namespace Org.Frontend.Services.Tasks
{
    public class TaskMockService : ITaskService
    {
        // Dùng static để data không bị reset khi chuyển trang
        private static List<TaskDto> _tasks = new()
        {
            new TaskDto { Id = Guid.NewGuid(), CategoryId = Guid.Empty, Title = "Lên kịch bản MC", Status = "TODO", AssigneeName = "Sarah J.", DueDate = DateTime.Today.AddDays(2) },
            new TaskDto { Id = Guid.NewGuid(), CategoryId = Guid.Empty, Title = "Thiết kế Backdrop", Status = "TODO", AssigneeName = "Marcus V.", DueDate = DateTime.Today.AddDays(3) },
            new TaskDto { Id = Guid.NewGuid(), CategoryId = Guid.Empty, Title = "Thuê dàn âm thanh ánh sáng", Status = "IN_PROGRESS", AssigneeName = "David C.", DueDate = DateTime.Today.AddDays(1) },
            new TaskDto { Id = Guid.NewGuid(), CategoryId = Guid.Empty, Title = "Khảo sát hội trường", Status = "DONE", AssigneeName = "David C.", DueDate = DateTime.Today.AddDays(-1) }
        };

        public Task<List<TaskDto>> GetTasksAsync(Guid categoryId)
        {
            // Tạm thời trả về tất cả task tĩnh để test UI kéo thả
            return Task.FromResult(_tasks.ToList()); 
        }

        public Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequest req)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.Status = req.Status;
            }
            return Task.CompletedTask;
        }

        public Task<TaskDto> CreateTaskAsync(Guid categoryId, CreateTaskRequest req)
        {
            var newTask = new TaskDto
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                Title = req.Title ?? "New Task",
                Status = "TODO",
                AssigneeName = "Chưa gán",
                DueDate = req.DueDate
            };
            _tasks.Insert(0, newTask); // Thêm lên đầu danh sách
            return Task.FromResult(newTask);
        }
    }
}