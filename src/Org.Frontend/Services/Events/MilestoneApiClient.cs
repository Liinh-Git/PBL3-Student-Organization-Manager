// ---- API client thực cho module cột mốc — ánh xạ MilestoneDto sang MilestoneViewModel ----
// Chuyển DateOnly từ API về DateTime để UI có thể so sánh và hiển thị.
using System.Net.Http.Json;
using Org.Shared;
using Org.Frontend.ViewModels;
using Org.Shared.Features.Milestones;

namespace Org.Frontend.Services.Events;

public sealed class MilestoneApiClient(HttpClient httpClient) : IMilestoneService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<MilestoneViewModel>> GetMilestonesAsync(Guid eventId)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetMilestonesResponse>($"api/events/{eventId}/milestones")
            ?? new GetMilestonesResponse([]);

        return payload.Items
            .Select(MapMilestone)
            .ToList();
    }

    public async Task<MilestoneViewModel> CreateMilestoneAsync(CreateMilestoneViewModel req)
    {
        var startDate = DateOnly.FromDateTime(req.StartDate?.Date ?? DateTime.Today);
        var endDate = DateOnly.FromDateTime(req.EndDate?.Date ?? req.StartDate?.Date ?? DateTime.Today);

        var payload = new CreateMilestoneRequest(
            req.EventId,
            NormalizeName(req.Name),
            null,
            startDate,
            endDate,
            req.OrderIndex);

        using var response = await _httpClient.PostAsJsonAsync($"api/events/{req.EventId}/milestones", payload);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<MilestoneDto>()
            ?? throw new InvalidOperationException("API returned no milestone payload.");

        return MapMilestone(created);
    }

    public async Task<MilestoneViewModel> UpdateMilestoneAsync(Guid milestoneId, UpdateMilestoneViewModel req)
    {
        var startDate = DateOnly.FromDateTime(req.StartDate?.Date ?? DateTime.Today);
        var endDate = DateOnly.FromDateTime(req.EndDate?.Date ?? req.StartDate?.Date ?? DateTime.Today);

        var payload = new UpdateMilestoneRequest(
            NormalizeName(req.Name),
            null,
            startDate,
            endDate,
            req.OrderIndex,
            ResolveStatus(startDate, endDate));

        using var response = await _httpClient.PutAsJsonAsync($"api/milestones/{milestoneId}", payload);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<MilestoneDto>()
            ?? throw new InvalidOperationException("API returned no milestone payload.");

        return MapMilestone(updated);
    }

    public async Task DeleteMilestoneAsync(Guid milestoneId)
    {
        using var response = await _httpClient.DeleteAsync($"api/milestones/{milestoneId}");
        response.EnsureSuccessStatusCode();
    }

    private static MilestoneViewModel MapMilestone(MilestoneDto x)
    {
        return new MilestoneViewModel
        {
            Id = x.Id,
            EventId = x.EventId,
            Name = x.Name,
            StartDate = x.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = x.EndDate.ToDateTime(TimeOnly.MinValue),
            OrderIndex = x.SortOrder
        };
    }

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Untitled Milestone";
        }

        return name.Trim();
    }

    private static MilestoneStatus ResolveStatus(DateOnly startDate, DateOnly endDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (endDate < today)
        {
            return MilestoneStatus.Completed;
        }

        if (startDate <= today)
        {
            return MilestoneStatus.InProgress;
        }

        return MilestoneStatus.NotStarted;
    }
}
