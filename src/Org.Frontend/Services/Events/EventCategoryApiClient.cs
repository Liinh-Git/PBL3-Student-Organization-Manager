// ---- API client thực cho module hạng mục sự kiện — ánh xạ EventCategoryDto sang ViewModel ----
// ProgressPercentage: tính phần trăm hoàn thành từ (CompletedTaskCount / TaskCount) * 100.
using System.Net.Http.Json;
using Org.Frontend.ViewModels;
using Org.Shared.Features.EventCategories;

namespace Org.Frontend.Services.Events;

public sealed class EventCategoryApiClient(HttpClient httpClient) : IEventCategoryService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<EventCategoryViewModel>> GetCategoriesAsync(Guid milestoneId)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetEventCategoriesResponse>($"api/milestones/{milestoneId}/categories")
            ?? new GetEventCategoriesResponse([]);

        return payload.Items
            .Select(MapCategory)
            .ToList();
    }

    public async Task<EventCategoryViewModel> GetCategoryDetailAsync(Guid categoryId)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetEventCategoryByIdResponse>($"api/categories/{categoryId}")
            ?? throw new InvalidOperationException("API returned no category detail payload.");

        return MapCategory(payload.Data);
    }

    public async Task<EventCategoryViewModel> CreateCategoryAsync(CreateEventCategoryViewModel req)
    {
        var existing = await _httpClient.GetFromJsonAsync<GetEventCategoriesResponse>($"api/milestones/{req.MilestoneId}/categories")
            ?? new GetEventCategoriesResponse([]);

        var payload = new CreateEventCategoryRequest(
            req.MilestoneId,
            NormalizeName(req.Name),
            null,
            existing.Items.Count);

        using var response = await _httpClient.PostAsJsonAsync($"api/milestones/{req.MilestoneId}/categories", payload);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<EventCategoryDto>()
            ?? throw new InvalidOperationException("API returned no category payload.");

        return MapCategory(created);
    }

    public async Task<EventCategoryViewModel> UpdateCategoryAsync(Guid categoryId, UpdateEventCategoryViewModel req)
    {
        var current = await _httpClient.GetFromJsonAsync<GetEventCategoryByIdResponse>($"api/categories/{categoryId}")
            ?? throw new InvalidOperationException("API returned no category detail payload.");

        var payload = new UpdateEventCategoryRequest(
            NormalizeName(req.Name),
            current.Data.Description,
            current.Data.SortOrder);

        using var response = await _httpClient.PutAsJsonAsync($"api/categories/{categoryId}", payload);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<EventCategoryDto>()
            ?? throw new InvalidOperationException("API returned no category payload.");

        return MapCategory(updated);
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        using var response = await _httpClient.DeleteAsync($"api/categories/{categoryId}");
        response.EnsureSuccessStatusCode();
    }

    private static EventCategoryViewModel MapCategory(EventCategoryDto x)
    {
        return new EventCategoryViewModel
        {
            Id = x.Id,
            MilestoneId = x.MilestoneId,
            Name = x.Name,
            Description = x.Description,
            LeadName = string.IsNullOrWhiteSpace(x.LeadName) ? "Chưa gán" : x.LeadName,
            LeadAvatarUrl = null,
            ActiveSubtasks = Math.Max(0, x.TaskCount - x.CompletedTaskCount),
            ProgressPercentage = x.TaskCount == 0
                ? 0
                : (int)Math.Round((double)x.CompletedTaskCount * 100 / x.TaskCount, MidpointRounding.AwayFromZero)
        };
    }

    private static string NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Untitled Category";
        }

        return value.Trim();
    }
}
