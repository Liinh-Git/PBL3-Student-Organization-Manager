using System.Net.Http.Json;
using Org.Frontend.ViewModels;
using Org.Shared.Features.EventCategories;

namespace Org.Frontend.Services.EventCategories;

public sealed class EventCategoryApiClient(HttpClient httpClient) : IEventCategoryService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<EventCategoryViewModel>> GetCategoriesAsync(Guid milestoneId)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetEventCategoriesResponse>($"api/milestones/{milestoneId}/categories")
            ?? new GetEventCategoriesResponse([]);

        return payload.Items
            .Select(x => new EventCategoryViewModel
            {
                Id = x.Id,
                MilestoneId = x.MilestoneId,
                Name = x.Name,
                LeadName = "Chua gan",
                LeadAvatarUrl = null,
                ActiveSubtasks = Math.Max(0, x.TaskCount - x.CompletedTaskCount),
                ProgressPercentage = x.TaskCount == 0
                    ? 0
                    : (int)Math.Round((double)x.CompletedTaskCount * 100 / x.TaskCount, MidpointRounding.AwayFromZero)
            })
            .ToList();
    }
}
