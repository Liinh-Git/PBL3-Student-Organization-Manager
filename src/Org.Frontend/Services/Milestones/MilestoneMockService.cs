// ---- MilestoneMockService ----
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Milestones;

public class MilestoneMockService : IMilestoneService
{
    public Task<List<MilestoneViewModel>> GetMilestonesAsync(Guid eventId)
    {
        return Task.FromResult(new List<MilestoneViewModel>
        {
            new() { Id = Guid.NewGuid(), EventId = eventId, Name = "Giai đoạn 1: Lên kế hoạch & Chuẩn bị", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(7), OrderIndex = 1 },
            new() { Id = Guid.NewGuid(), EventId = eventId, Name = "Giai đoạn 2: Triển khai sự kiện", StartDate = DateTime.Today.AddDays(8), EndDate = DateTime.Today.AddDays(14), OrderIndex = 2 }
        });
    }
}