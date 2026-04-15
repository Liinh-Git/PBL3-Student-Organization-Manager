using System.Net.Http.Json;
using Org.Frontend.ViewModels;
using Org.Shared.Features.Milestones;

namespace Org.Frontend.Services.Milestones;

public sealed class MilestoneApiClient(HttpClient httpClient) : IMilestoneService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<MilestoneViewModel>> GetMilestonesAsync(Guid eventId)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetMilestonesResponse>($"api/events/{eventId}/milestones")
            ?? new GetMilestonesResponse([]);

        return payload.Items
            .Select(x => new MilestoneViewModel
            {
                Id = x.Id,
                EventId = x.EventId,
                Name = x.Name,
                StartDate = x.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = x.EndDate.ToDateTime(TimeOnly.MinValue),
                OrderIndex = x.SortOrder
            })
            .ToList();
    }
}
