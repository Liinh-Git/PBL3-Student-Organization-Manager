// ---- Interface service milestones ----
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Events;

public interface IMilestoneService
{
    Task<List<MilestoneViewModel>> GetMilestonesAsync(Guid eventId);
    Task<MilestoneViewModel> CreateMilestoneAsync(CreateMilestoneViewModel req);
    Task<MilestoneViewModel> UpdateMilestoneAsync(Guid milestoneId, UpdateMilestoneViewModel req);
    Task DeleteMilestoneAsync(Guid milestoneId);
}
