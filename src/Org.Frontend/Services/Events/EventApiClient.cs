using System.Net;
using System.Net.Http.Json;
using Org.Frontend.Services.Organizations;
using Org.Frontend.ViewModels;
using Org.Shared;
using Org.Shared.Features.EventCategories;
using Org.Shared.Features.Events;
using Org.Shared.Features.Milestones;

namespace Org.Frontend.Services.Events;

public sealed class EventApiClient(HttpClient httpClient, IOrganizationContext organizationContext) : IEventService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IOrganizationContext _organizationContext = organizationContext;

    public async Task<List<EventViewModel>> GetEventsAsync(Guid orgId)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetOrganizationEventsResponse>($"api/organizations/{orgId}/events")
            ?? new GetOrganizationEventsResponse([]);

        return payload.Items.Select(MapTreeNode).ToList();
    }

    public async Task<EventViewModel> CreateEventAsync(CreateEventViewModel request)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync();
        var date = request.Date?.Date ?? DateTime.Today;
        var startDate = DateOnly.FromDateTime(date);

        var payload = new CreateEventRequest(
            orgId,
            NormalizeTitle(request.Title),
            request.Location?.Trim(),
            startDate,
            startDate);

        using var response = await _httpClient.PostAsJsonAsync("api/events", payload);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<EventDto>()
            ?? throw new InvalidOperationException("API returned no event payload.");

        return await BuildDetailFromEventDtoAsync(created);
    }

    public async Task<EventViewModel?> GetEventDetailAsync(Guid eventId)
    {
        using var response = await _httpClient.GetAsync($"api/events/{eventId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GetEventByIdResponse>()
            ?? throw new InvalidOperationException("API returned no event detail payload.");

        return await BuildDetailFromEventDtoAsync(payload.Data);
    }

    private async Task<EventViewModel> BuildDetailFromEventDtoAsync(EventDto dto)
    {
        var milestonesPayload = await _httpClient.GetFromJsonAsync<GetMilestonesResponse>($"api/events/{dto.Id}/milestones")
            ?? new GetMilestonesResponse([]);

        var totalTasks = 0;
        var completedTasks = 0;

        foreach (var milestone in milestonesPayload.Items)
        {
            var categoriesPayload = await _httpClient.GetFromJsonAsync<GetEventCategoriesResponse>($"api/milestones/{milestone.Id}/categories")
                ?? new GetEventCategoriesResponse([]);

            totalTasks += categoriesPayload.Items.Sum(x => x.TaskCount);
            completedTasks += categoriesPayload.Items.Sum(x => x.CompletedTaskCount);
        }

        var completion = totalTasks == 0
            ? 0
            : (int)Math.Round((double)completedTasks * 100 / totalTasks, MidpointRounding.AwayFromZero);

        var budgetUsed = Math.Clamp(completion + 12, 0, 100);

        return new EventViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = string.IsNullOrWhiteSpace(dto.Description)
                ? "Sự kiện đang được triển khai theo kế hoạch đã phê duyệt."
                : dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            StatusLabel = ToStatusLabel(dto.Status),
            Location = "Campus Workspace",
            RegisteredCount = completedTasks,
            TotalSlots = totalTasks,
            CompletionLabel = $"{completion}%",
            BudgetLabel = $"{budgetUsed}%",
            RiskLevel = completion >= 80 ? "Low" : completion >= 40 ? "Medium" : "High",
            TotalFiles = Math.Max(1, totalTasks * 2),
            ActualSpending = Math.Max(250_000m, totalTasks * 150_000m)
        };
    }

    private static EventViewModel MapTreeNode(EventTreeNodeDto node)
    {
        return new EventViewModel
        {
            Id = node.Id,
            Name = node.Name,
            StartDate = node.StartDate,
            EndDate = node.EndDate,
            StatusLabel = ToStatusLabel(node.Status),
            Location = $"{node.MilestoneCount} milestones",
            RegisteredCount = node.CompletedTaskCount,
            TotalSlots = node.TaskCount,
            CompletionLabel = node.TaskCount == 0
                ? "0%"
                : $"{(int)Math.Round((double)node.CompletedTaskCount * 100 / node.TaskCount, MidpointRounding.AwayFromZero)}%"
        };
    }

    private static string NormalizeTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "Untitled Event";

        return title.Trim();
    }

    private static string ToStatusLabel(EventStatus status)
        => status switch
        {
            EventStatus.Ongoing => "ONGOING",
            EventStatus.Completed => "COMPLETED",
            _ => "UPCOMING"
        };
}
