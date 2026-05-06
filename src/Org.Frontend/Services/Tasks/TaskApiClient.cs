using System.Net.Http.Json;
using Org.Frontend.Services.Auth;
using Org.Frontend.ViewModels;
using Org.Shared;
using Org.Shared.Features.EventCategories;
using Org.Shared.Features.Events;
using Org.Shared.Features.Milestones;
using Org.Shared.Features.Tasks;
using Org.Shared.Features.Users;
using SharedTaskStatus = Org.Shared.TaskStatus;

namespace Org.Frontend.Services.Tasks;

public sealed class TaskApiClient(IAuthenticatedBackendClient backendClient) : ITaskService
{
    private readonly IAuthenticatedBackendClient _backendClient = backendClient;

    public async Task<List<TaskViewModel>> GetTasksAsync(Guid categoryId)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetTasksResponse>($"api/categories/{categoryId}/tasks")
            ?? new GetTasksResponse([]);

        return payload.Items
            .Select(MapTask)
            .ToList();
    }

    public async Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusViewModel req)
    {
        var payload = new UpdateTaskStatusRequest(ParseStatus(req.Status));
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/tasks/{taskId}/status")
        {
            Content = JsonContent.Create(payload)
        };

        using var _ = await _backendClient.SendAsync(request);
    }

    public async Task<TaskViewModel> CreateTaskAsync(Guid categoryId, CreateTaskViewModel req)
    {
        var assigneeIds = req.AssigneeMemberIds ?? [];
        if (assigneeIds.Count > 1)
        {
            throw new NotSupportedException(
                "Live API currently supports only one assignee per task. Multiple assignees are available in mock mode.");
        }

        var payload = new CreateTaskRequest(
            categoryId,
            NormalizeTitle(req.Title),
            null,
            assigneeIds.FirstOrDefault(),
            req.DueDate is null ? null : DateOnly.FromDateTime(req.DueDate.Value),
            ParsePriority(req.Priority));

        var created = await _backendClient.PostAsJsonAsync<CreateTaskRequest, TaskDto>(
            $"api/categories/{categoryId}/tasks",
            payload) ?? throw new InvalidOperationException("API returned no task payload.");

        return MapTask(created);
    }

    public async Task<bool> CanManageTasksAsync(Guid categoryId)
    {
        var category = await _backendClient.GetFromJsonAsync<GetEventCategoryByIdResponse>($"api/categories/{categoryId}")
            ?? throw new InvalidOperationException("API returned no category detail payload.");

        var milestone = await _backendClient.GetFromJsonAsync<GetMilestoneByIdResponse>($"api/milestones/{category.Data.MilestoneId}")
            ?? throw new InvalidOperationException("API returned no milestone detail payload.");

        var @event = await _backendClient.GetFromJsonAsync<GetEventByIdResponse>($"api/events/{milestone.Data.EventId}")
            ?? throw new InvalidOperationException("API returned no event detail payload.");

        var myOrgs = await _backendClient.GetFromJsonAsync<GetMyOrganizationsResponse>("api/users/me/organizations")
            ?? new GetMyOrganizationsResponse([]);

        var myRole = myOrgs.Items.FirstOrDefault(x => x.OrganizationId == @event.Data.OrganizationId)?.MemberRole;
        return myRole?.Trim().ToUpperInvariant() is "PRESIDENT" or "VICEPRESIDENT" or "MANAGER" or "OWNER" or "ADMIN";
    }

    private static TaskViewModel MapTask(TaskDto task)
    {
        return new TaskViewModel
        {
            Id = task.Id,
            CategoryId = task.CategoryId,
            Title = task.Title,
            Status = ToStatusText(task.Status),
            Priority = task.Priority.ToString().ToUpperInvariant(),
            AssigneeMemberId = task.AssigneeMemberId,
            AssigneeName = task.AssigneeMemberId.HasValue
                ? $"MEM-{task.AssigneeMemberId.Value.ToString("N")[..6].ToUpperInvariant()}"
                : "Unassigned",
            DueDate = task.DueDate?.ToDateTime(TimeOnly.MinValue),
            CanMarkCompleted = false
        };
    }

    private static string NormalizeTitle(string? title)
        => string.IsNullOrWhiteSpace(title) ? "New Task" : title.Trim();

    private static SharedTaskStatus ParseStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "IN_PROGRESS" => SharedTaskStatus.InProgress,
            "DONE" => SharedTaskStatus.Done,
            _ => SharedTaskStatus.Todo
        };
    }

    private static TaskPriority ParsePriority(string? priority)
    {
        return priority?.Trim().ToUpperInvariant() switch
        {
            "LOW" => TaskPriority.Low,
            "HIGH" => TaskPriority.High,
            "URGENT" => TaskPriority.High,
            _ => TaskPriority.Medium
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
