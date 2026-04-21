// ---- API client thực cho module nhiệm vụ — ánh xạ TaskDto sang TaskViewModel ----
// ParseStatus / ToStatusText: bridge giữa chuỗi UI (TODO/IN_PROGRESS/DONE) và enum Shared.TaskStatus.
using System.Net.Http.Json;
using Org.Frontend.ViewModels;
using Org.Shared;
using Org.Shared.Features.Tasks;
using SharedTaskStatus = Org.Shared.TaskStatus;

namespace Org.Frontend.Services.Tasks;

public sealed class TaskApiClient(HttpClient httpClient) : ITaskService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<TaskViewModel>> GetTasksAsync(Guid categoryId)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetTasksResponse>($"api/categories/{categoryId}/tasks")
            ?? new GetTasksResponse([]);

        return payload.Items
            .Select(MapTask)
            .ToList();
    }

    public async Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusViewModel req)
    {
        var payload = new UpdateTaskStatusRequest(ParseStatus(req.Status));
        using var response = await _httpClient.PutAsJsonAsync($"api/tasks/{taskId}/status", payload);
        response.EnsureSuccessStatusCode();
    }

    public async Task<TaskViewModel> CreateTaskAsync(Guid categoryId, CreateTaskViewModel req)
    {
        var payload = new CreateTaskRequest(
            categoryId,
            NormalizeTitle(req.Title),
            null,
            null,
            req.DueDate is null ? null : DateOnly.FromDateTime(req.DueDate.Value),
            TaskPriority.Medium);

        using var response = await _httpClient.PostAsJsonAsync($"api/categories/{categoryId}/tasks", payload);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<TaskDto>()
            ?? throw new InvalidOperationException("API returned no task payload.");

        return MapTask(created);
    }

    private static TaskViewModel MapTask(TaskDto task)
    {
        return new TaskViewModel
        {
            Id = task.Id,
            CategoryId = task.CategoryId,
            Title = task.Title,
            Status = ToStatusText(task.Status),
            AssigneeName = task.AssigneeMemberId.HasValue
                ? $"MEM-{task.AssigneeMemberId.Value.ToString("N")[..6].ToUpperInvariant()}"
                : "Unassigned",
            DueDate = task.DueDate?.ToDateTime(TimeOnly.MinValue)
        };
    }

    private static string NormalizeTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "New Task";

        return title.Trim();
    }

    private static SharedTaskStatus ParseStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "IN_PROGRESS" => SharedTaskStatus.InProgress,
            "DONE" => SharedTaskStatus.Done,
            _ => SharedTaskStatus.Todo
        };
    }

    private static string ToStatusText(SharedTaskStatus status)
    {
        return status switch
        {
            SharedTaskStatus.InProgress => "IN_PROGRESS",
            SharedTaskStatus.Done => "DONE",
            _ => "TODO"
        };
    }
}
