// ---- Interface service milestones ----
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Milestones;

public interface IMilestoneService
{
    Task<List<MilestoneViewModel>> GetMilestonesAsync(Guid eventId);
}