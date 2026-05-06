using Org.Frontend.Services.Auth;
using Org.Frontend.ViewModels;
using Org.Shared.Features.EventCategories;

namespace Org.Frontend.Services.Events;

public sealed class EventCategoryApiClient(IAuthenticatedBackendClient backendClient) : IEventCategoryService
{
    private readonly IAuthenticatedBackendClient _backendClient = backendClient;

    public async Task<List<EventCategoryViewModel>> GetCategoriesAsync(Guid milestoneId)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetEventCategoriesResponse>($"api/milestones/{milestoneId}/categories")
            ?? new GetEventCategoriesResponse([]);

        return payload.Items
            .Select(MapCategory)
            .ToList();
    }

    public async Task<EventCategoryViewModel> GetCategoryDetailAsync(Guid categoryId)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetEventCategoryByIdResponse>($"api/categories/{categoryId}")
            ?? throw new InvalidOperationException("API returned no category detail payload.");

        return MapCategory(payload.Data);
    }

    public async Task<EventCategoryViewModel> CreateCategoryAsync(CreateEventCategoryViewModel req)
    {
        var existing = await _backendClient.GetFromJsonAsync<GetEventCategoriesResponse>($"api/milestones/{req.MilestoneId}/categories")
            ?? new GetEventCategoriesResponse([]);

        var payload = new CreateEventCategoryRequest(
            req.MilestoneId,
            NormalizeName(req.Name),
            null,
            existing.Items.Count);

        var created = await _backendClient.PostAsJsonAsync<CreateEventCategoryRequest, EventCategoryDto>(
            $"api/milestones/{req.MilestoneId}/categories",
            payload) ?? throw new InvalidOperationException("API returned no category payload.");

        return MapCategory(created);
    }

    public async Task<EventCategoryViewModel> UpdateCategoryAsync(Guid categoryId, UpdateEventCategoryViewModel req)
    {
        var current = await _backendClient.GetFromJsonAsync<GetEventCategoryByIdResponse>($"api/categories/{categoryId}")
            ?? throw new InvalidOperationException("API returned no category detail payload.");

        var payload = new UpdateEventCategoryRequest(
            NormalizeName(req.Name),
            current.Data.Description,
            current.Data.SortOrder);

        var updated = await _backendClient.PutAsJsonAsync<UpdateEventCategoryRequest, EventCategoryDto>(
            $"api/categories/{categoryId}",
            payload) ?? throw new InvalidOperationException("API returned no category payload.");

        return MapCategory(updated);
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        await _backendClient.DeleteAsync($"api/categories/{categoryId}");
    }

    private static EventCategoryViewModel MapCategory(EventCategoryDto x)
    {
        var responsible = new List<CategoryMemberViewModel>();
        if (!string.IsNullOrWhiteSpace(x.LeadName))
        {
            responsible.Add(new CategoryMemberViewModel
            {
                MemberId = null,
                Name = x.LeadName,
                Role = "Category Lead"
            });
        }

        return new EventCategoryViewModel
        {
            Id = x.Id,
            MilestoneId = x.MilestoneId,
            Name = x.Name,
            Description = x.Description,
            LeadName = string.IsNullOrWhiteSpace(x.LeadName) ? "Chua gan" : x.LeadName,
            LeadAvatarUrl = null,
            ActiveSubtasks = Math.Max(0, x.TaskCount - x.CompletedTaskCount),
            ProgressPercentage = x.TaskCount == 0
                ? 0
                : (int)Math.Round((double)x.CompletedTaskCount * 100 / x.TaskCount, MidpointRounding.AwayFromZero),

            DetailedDescription = x.Description,
            Guidelines = [],
            IsUrgent = false,
            ResponsibleMembers = responsible
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
