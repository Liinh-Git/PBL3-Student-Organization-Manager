using System.Net;
using System.Net.Http.Json;
using Org.Frontend.Services.Organizations;
using Org.Frontend.ViewModels;
using Org.Shared;
using Org.Shared.Features.EventCategories;
using Org.Shared.Features.Events;
using Org.Shared.Features.Milestones;
using Org.Shared.Features.Users;

namespace Org.Frontend.Services.Events;

public sealed class EventApiClient(HttpClient httpClient, IOrganizationContext organizationContext) : IEventService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IOrganizationContext _organizationContext = organizationContext;

    public async Task<List<EventViewModel>> GetEventsAsync(Guid orgId)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetOrganizationEventsResponse>($"api/organizations/{orgId}/events")
            ?? new GetOrganizationEventsResponse([]);

        var canManage = await CanCreateEventAsync(orgId);
        return payload.Items
            .Select(node => MapTreeNode(node, orgId, canManage))
            .ToList();
    }

    public async Task<MyEventsViewModel> GetMyEventsAsync()
    {
        var organizationsPayload = await _httpClient.GetFromJsonAsync<GetMyOrganizationsResponse>(
            "api/users/me/organizations") ?? new GetMyOrganizationsResponse([]);

        var registeredPayload = await _httpClient.GetFromJsonAsync<GetMyRegisteredEventsResponse>(
            "api/users/me/events") ?? new GetMyRegisteredEventsResponse([]);

        var organizerEvents = new List<MyEventItemViewModel>();
        var organizerRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PRESIDENT",
            "VICEPRESIDENT",
            "MANAGER",
            "OWNER",
            "ADMIN"
        };

        foreach (var org in organizationsPayload.Items.Where(x => organizerRoles.Contains(x.MemberRole?.Trim() ?? string.Empty)))
        {
            var eventsPayload = await _httpClient.GetFromJsonAsync<GetOrganizationEventsResponse>(
                $"api/organizations/{org.OrganizationId}/events") ?? new GetOrganizationEventsResponse([]);

            organizerEvents.AddRange(eventsPayload.Items.Select(x => new MyEventItemViewModel
            {
                EventId = x.Id,
                OrganizationId = org.OrganizationId,
                OrganizationName = org.OrganizationName,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                StatusLabel = ToStatusLabel(x.Status),
                Location = null,
                IsOrganizer = true,
                CanEnterWorkspace = true,
                CanManage = true
            }));
        }

        var organizerEventIds = organizerEvents.Select(x => x.EventId).ToHashSet();

        var attendeeEvents = registeredPayload.Items
            .Where(x => !organizerEventIds.Contains(x.EventId))
            .Select(x => new MyEventItemViewModel
            {
                EventId = x.EventId,
                OrganizationId = x.OrganizationId,
                OrganizationName = x.OrganizationName,
                Name = x.EventName,
                Description = x.EventDescription,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                StatusLabel = ToStatusLabel(x.EventStatus),
                Location = x.Location,
                ImageUrl = x.EventImageUrl,
                IsOrganizer = false,
                CanEnterWorkspace = false,
                CanManage = false
            })
            .OrderBy(x => x.StartDate)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new MyEventsViewModel
        {
            OrganizerEvents = organizerEvents
                .DistinctBy(x => x.EventId)
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            AttendeeEvents = attendeeEvents
        };
    }

    public async Task<EventViewModel?> GetPublicEventDetailAsync(Guid eventId)
    {
        using var response = await _httpClient.GetAsync($"api/events/{eventId:D}/public");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<GetEventByIdResponse>()
            ?? throw new InvalidOperationException("API returned no public event detail payload.");

        return await BuildDetailFromEventDtoAsync(payload.Data);
    }

    public async Task<bool> CanCreateEventAsync(Guid orgId)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetMyOrganizationsResponse>(
            "api/users/me/organizations") ?? new GetMyOrganizationsResponse([]);

        var member = payload.Items.FirstOrDefault(x => x.OrganizationId == orgId);
        if (member is null)
            return false;

        return member.MemberRole.Trim().ToUpperInvariant() switch
        {
            "PRESIDENT" => true,
            "VICEPRESIDENT" => true,
            "MANAGER" => true,
            "OWNER" => true,
            "ADMIN" => true,
            _ => false
        };
    }

    public async Task<bool> CanManageEventAsync(Guid eventId)
    {
        using var response = await _httpClient.GetAsync($"api/events/{eventId}");
        if (!response.IsSuccessStatusCode)
            return false;

        var payload = await response.Content.ReadFromJsonAsync<GetEventByIdResponse>()
            ?? throw new InvalidOperationException("API returned no event detail payload.");

        return await CanCreateEventAsync(payload.Data.OrganizationId);
    }

    public async Task<EventViewModel> CreateEventAsync(CreateEventViewModel request)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync();

        var startDate = DateOnly.FromDateTime((request.StartDate ?? DateTime.Today).Date);
        var endDate = DateOnly.FromDateTime((request.EndDate ?? request.StartDate ?? DateTime.Today).Date);
        if (endDate < startDate)
            endDate = startDate;

        var payload = new CreateEventRequest(
            orgId,
            NormalizeTitle(request.Title),
            request.Description?.Trim(),
            startDate,
            endDate,
            NormalizeTags(request.Tags));

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

    public async Task<EventViewModel> UpdateEventAsync(Guid eventId, UpdateEventViewModel req)
    {
        var current = await _httpClient.GetFromJsonAsync<GetEventByIdResponse>($"api/events/{eventId}")
            ?? throw new InvalidOperationException("API returned no event detail payload.");

        var startDate = req.StartDate.HasValue
            ? DateOnly.FromDateTime(req.StartDate.Value.Date)
            : current.Data.StartDate;

        var endDate = req.EndDate.HasValue
            ? DateOnly.FromDateTime(req.EndDate.Value.Date)
            : current.Data.EndDate;

        if (endDate < startDate)
            endDate = startDate;

        var payload = new UpdateEventRequest(
            string.IsNullOrWhiteSpace(req.Name) ? current.Data.Name : req.Name.Trim(),
            req.Description?.Trim() ?? current.Data.Description,
            startDate,
            endDate,
            current.Data.Status,
            current.Data.Tags);

        using var response = await _httpClient.PutAsJsonAsync($"api/events/{eventId}", payload);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<EventDto>()
            ?? throw new InvalidOperationException("API returned no event payload.");

        return await BuildDetailFromEventDtoAsync(updated);
    }

    public async Task DeleteEventAsync(Guid eventId)
    {
        using var response = await _httpClient.DeleteAsync($"api/events/{eventId}");
        response.EnsureSuccessStatusCode();
    }

    public Task RegisterEventAsync(Guid eventId)
        => throw new NotSupportedException("Live API endpoint for event registration is not available yet.");

    public Task UnregisterEventAsync(Guid eventId)
        => throw new NotSupportedException("Live API endpoint for event unregistration is not available yet.");

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

        return new EventViewModel
        {
            Id = dto.Id,
            OrganizationId = dto.OrganizationId,
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            StatusLabel = ToStatusLabel(dto.Status),
            Location = null,
            RegisteredCount = completedTasks,
            TotalSlots = totalTasks,
            CompletionLabel = $"{completion}%",
            CompletionPercentage = completion,
            BudgetLabel = null,
            RiskLevel = null,
            TotalFiles = 0,
            ActualSpending = 0,
            CanManage = await CanCreateEventAsync(dto.OrganizationId),
            CanEnterWorkspace = await CanCreateEventAsync(dto.OrganizationId)
        };
    }

    private static EventViewModel MapTreeNode(EventTreeNodeDto node, Guid orgId, bool canManage)
    {
        var completion = node.TaskCount == 0
            ? 0
            : (int)Math.Round((double)node.CompletedTaskCount * 100 / node.TaskCount, MidpointRounding.AwayFromZero);

        return new EventViewModel
        {
            Id = node.Id,
            OrganizationId = orgId,
            Name = node.Name,
            StartDate = node.StartDate,
            EndDate = node.EndDate,
            StatusLabel = ToStatusLabel(node.Status),
            Location = null,
            RegisteredCount = node.CompletedTaskCount,
            TotalSlots = node.TaskCount,
            CompletionLabel = $"{completion}%",
            CompletionPercentage = completion,
            CanManage = canManage,
            CanEnterWorkspace = canManage
        };
    }

    private static string NormalizeTitle(string? title)
        => string.IsNullOrWhiteSpace(title) ? string.Empty : title.Trim();

    private static IReadOnlyList<string> NormalizeTags(IReadOnlyList<string>? tags)
    {
        if (tags is null)
            return Array.Empty<string>();

        return tags.Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string ToStatusLabel(EventStatus status)
        => status switch
        {
            EventStatus.Ongoing => "ONGOING",
            EventStatus.Completed => "COMPLETED",
            _ => "UPCOMING"
        };
}
