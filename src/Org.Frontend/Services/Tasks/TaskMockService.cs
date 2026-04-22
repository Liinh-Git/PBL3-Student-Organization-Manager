// ---- TaskMockService ----
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Tasks;

public sealed class TaskMockService(FrontendMockDataStore mockDataStore) : ITaskService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;

    public Task<List<TaskViewModel>> GetTasksAsync(Guid categoryId)
    {
        return _mockDataStore.UseAsync(data => data.Tasks
            .Where(x => x.CategoryId == categoryId)
            .OrderBy(x => x.DueDate ?? DateTime.MaxValue)
            .ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .Select(x => MapTask(data, x))
            .ToList());
    }

    public async Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusViewModel req)
    {
        await _mockDataStore.UseAsync(data =>
        {
            var task = data.Tasks.FirstOrDefault(x => x.Id == taskId)
                ?? throw new KeyNotFoundException($"Task {taskId} not found in mock data.");

            task.Status = NormalizeStatus(req.Status);
            return 0;
        });
    }

    public Task<TaskViewModel> CreateTaskAsync(Guid categoryId, CreateTaskViewModel req)
    {
        return _mockDataStore.UseAsync(data =>
        {
            if (!data.EventCategories.Any(x => x.Id == categoryId))
            {
                throw new KeyNotFoundException($"Category {categoryId} not found in mock data.");
            }

            var item = new MockTask
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                Title = NormalizeTitle(req.Title),
                Status = "TODO",
                AssigneeMemberId = null,
                DueDate = req.DueDate,
                Note = null
            };

            data.Tasks.Insert(0, item);
            return MapTask(data, item);
        });
    }

    private static TaskViewModel MapTask(MockDataSet data, MockTask source)
    {
        var assigneeName = "Unassigned";
        if (source.AssigneeMemberId.HasValue)
        {
            assigneeName = data.Members.FirstOrDefault(x => x.Id == source.AssigneeMemberId.Value)?.DisplayName
                ?? "Unassigned";
        }

        return new TaskViewModel
        {
            Id = source.Id,
            CategoryId = source.CategoryId,
            Title = source.Title,
            Status = NormalizeStatus(source.Status),
            AssigneeName = assigneeName,
            DueDate = source.DueDate
        };
    }

    private static string NormalizeTitle(string? title)
        => string.IsNullOrWhiteSpace(title) ? "New Task" : title.Trim();

    private static string NormalizeStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "IN_PROGRESS" => "IN_PROGRESS",
            "DONE" => "DONE",
            _ => "TODO"
        };
    }
}