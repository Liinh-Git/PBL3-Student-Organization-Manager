using Org.Shared.Features.Milestones;

namespace Org.Backend.Features.Milestones.Services;

public interface IMilestoneService
{
    Task<List<MilestoneDto>> GetEventMilestonesAsync(Guid eventId, Guid userId, CancellationToken ct = default);
    Task<MilestoneDto> GetMilestoneByIdAsync(Guid milestoneId, Guid userId, CancellationToken ct = default);
    Task<MilestoneDto> CreateMilestoneAsync(Guid eventId, CreateMilestoneRequest request, Guid userId, CancellationToken ct = default);
    Task<MilestoneDto> UpdateMilestoneAsync(Guid milestoneId, UpdateMilestoneRequest request, Guid userId, CancellationToken ct = default);
    Task DeleteMilestoneAsync(Guid milestoneId, Guid userId, CancellationToken ct = default);
}
