// ---- MilestoneMockService ----
using Org.Frontend.Services.Mocks;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Events;

public sealed class MilestoneMockService(FrontendMockDataStore mockDataStore) : IMilestoneService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;

    public Task<List<MilestoneViewModel>> GetMilestonesAsync(Guid eventId)
    {
        return _mockDataStore.UseAsync(data => data.Milestones
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.OrderIndex)
            .Select(x => new MilestoneViewModel
            {
                Id = x.Id,
                EventId = x.EventId,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                OrderIndex = x.OrderIndex
            })
            .ToList());
    }

    public Task<MilestoneViewModel> CreateMilestoneAsync(CreateMilestoneViewModel req)
    {
        return _mockDataStore.UseAsync(data =>
        {
            if (!data.Events.Any(x => x.Id == req.EventId))
            {
                throw new KeyNotFoundException($"Event {req.EventId} not found in mock data.");
            }

            var item = new Org.Frontend.Services.Mocks.Models.MockMilestone
            {
                Id = Guid.NewGuid(),
                EventId = req.EventId,
                Name = NormalizeName(req.Name),
                StartDate = req.StartDate?.Date ?? DateTime.Today,
                EndDate = req.EndDate?.Date ?? req.StartDate?.Date ?? DateTime.Today,
                OrderIndex = req.OrderIndex <= 0
                    ? data.Milestones.Where(x => x.EventId == req.EventId).Select(x => x.OrderIndex).DefaultIfEmpty(0).Max() + 1
                    : req.OrderIndex
            };

            data.Milestones.Add(item);
            return MapMilestone(item);
        });
    }

    public Task<MilestoneViewModel> UpdateMilestoneAsync(Guid milestoneId, UpdateMilestoneViewModel req)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var current = data.Milestones.FirstOrDefault(x => x.Id == milestoneId)
                ?? throw new KeyNotFoundException($"Milestone {milestoneId} not found in mock data.");

            current.Name = NormalizeName(req.Name);
            current.StartDate = req.StartDate?.Date ?? current.StartDate;
            current.EndDate = req.EndDate?.Date ?? current.EndDate;
            current.OrderIndex = req.OrderIndex;

            return MapMilestone(current);
        });
    }

    public async Task DeleteMilestoneAsync(Guid milestoneId)
    {
        await _mockDataStore.UseAsync(data =>
        {
            var current = data.Milestones.FirstOrDefault(x => x.Id == milestoneId)
                ?? throw new KeyNotFoundException($"Milestone {milestoneId} not found in mock data.");

            var categoryIds = data.EventCategories
                .Where(x => x.MilestoneId == milestoneId)
                .Select(x => x.Id)
                .ToHashSet();

            data.Tasks.RemoveAll(x => categoryIds.Contains(x.CategoryId));
            data.EventCategories.RemoveAll(x => x.MilestoneId == milestoneId);
            data.Milestones.Remove(current);

            return 0;
        });
    }

    private static MilestoneViewModel MapMilestone(Org.Frontend.Services.Mocks.Models.MockMilestone x)
    {
        return new MilestoneViewModel
        {
            Id = x.Id,
            EventId = x.EventId,
            Name = x.Name,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            OrderIndex = x.OrderIndex
        };
    }

    private static string NormalizeName(string? value)
        => string.IsNullOrWhiteSpace(value) ? "Untitled Milestone" : value.Trim();
}
