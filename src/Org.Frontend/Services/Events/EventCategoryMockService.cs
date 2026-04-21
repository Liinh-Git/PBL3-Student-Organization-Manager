// ---- EventCategoryMockService ----
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Events;

public sealed class EventCategoryMockService(FrontendMockDataStore mockDataStore) : IEventCategoryService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;

    public Task<List<EventCategoryViewModel>> GetCategoriesAsync(Guid milestoneId)
    {
        return _mockDataStore.UseAsync(data =>
        {
            return data.EventCategories
                .Where(x => x.MilestoneId == milestoneId)
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(category => MapCategory(data, category))
                .ToList();
        });
    }

    public Task<EventCategoryViewModel> GetCategoryDetailAsync(Guid categoryId)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var category = data.EventCategories.FirstOrDefault(x => x.Id == categoryId)
                ?? throw new KeyNotFoundException($"Category {categoryId} not found in mock data.");

            return MapCategory(data, category);
        });
    }

    public Task<EventCategoryViewModel> CreateCategoryAsync(CreateEventCategoryViewModel req)
    {
        return _mockDataStore.UseAsync(data =>
        {
            if (!data.Milestones.Any(x => x.Id == req.MilestoneId))
            {
                throw new KeyNotFoundException($"Milestone {req.MilestoneId} not found in mock data.");
            }

            var item = new MockEventCategory
            {
                Id = Guid.NewGuid(),
                MilestoneId = req.MilestoneId,
                Name = NormalizeName(req.Name),
                LeadMemberId = req.LeadMemberId,
                Description = null,
                OrderIndex = data.EventCategories.Where(x => x.MilestoneId == req.MilestoneId).Select(x => x.OrderIndex).DefaultIfEmpty(-1).Max() + 1,
                OwnerDepartmentId = null
            };

            data.EventCategories.Add(item);
            return MapCategory(data, item);
        });
    }

    public Task<EventCategoryViewModel> UpdateCategoryAsync(Guid categoryId, UpdateEventCategoryViewModel req)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var category = data.EventCategories.FirstOrDefault(x => x.Id == categoryId)
                ?? throw new KeyNotFoundException($"Category {categoryId} not found in mock data.");

            category.Name = NormalizeName(req.Name);
            category.LeadMemberId = req.LeadMemberId;

            return MapCategory(data, category);
        });
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        await _mockDataStore.UseAsync(data =>
        {
            var category = data.EventCategories.FirstOrDefault(x => x.Id == categoryId)
                ?? throw new KeyNotFoundException($"Category {categoryId} not found in mock data.");

            data.Tasks.RemoveAll(x => x.CategoryId == categoryId);
            data.EventCategories.Remove(category);
            return 0;
        });
    }

    private static EventCategoryViewModel MapCategory(MockDataSet data, MockEventCategory category)
    {
        var leadName = "Chưa gán";
        string? leadAvatarUrl = null;

        if (category.LeadMemberId.HasValue)
        {
            var member = data.Members.FirstOrDefault(x => x.Id == category.LeadMemberId.Value);
            if (member is not null)
            {
                leadName = member.DisplayName;
                leadAvatarUrl = data.Users.FirstOrDefault(x => x.Id == member.UserId)?.AvatarUrl;
            }
        }

        var categoryTasks = data.Tasks.Where(x => x.CategoryId == category.Id).ToList();
        var totalTasks = categoryTasks.Count;
        var completedTasks = categoryTasks.Count(x => string.Equals(x.Status, "DONE", StringComparison.OrdinalIgnoreCase));

        return new EventCategoryViewModel
        {
            Id = category.Id,
            MilestoneId = category.MilestoneId,
            Name = category.Name,
            Description = category.Description,
            LeadName = leadName,
            LeadAvatarUrl = leadAvatarUrl,
            ActiveSubtasks = Math.Max(0, totalTasks - completedTasks),
            ProgressPercentage = totalTasks == 0
                ? 0
                : (int)Math.Round((double)completedTasks * 100 / totalTasks, MidpointRounding.AwayFromZero)
        };
    }

    private static string NormalizeName(string? value)
        => string.IsNullOrWhiteSpace(value) ? "Untitled Category" : value.Trim();
}
