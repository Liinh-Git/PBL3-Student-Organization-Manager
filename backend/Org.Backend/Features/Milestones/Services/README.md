# Milestones Services

## IMilestoneService / MilestoneService
**Methods**:
- `Task<List<MilestoneDto>> ListMilestonesAsync(Guid eventId, Guid userId)`
- `Task<MilestoneDto> CreateMilestoneAsync(Guid eventId, CreateMilestoneRequest request, Guid userId)`
- `Task<MilestoneDto> GetMilestoneAsync(Guid milestoneId, Guid userId)`
- `Task<MilestoneDto> UpdateMilestoneAsync(Guid milestoneId, UpdateMilestoneRequest request, Guid userId)`
- `Task DeleteMilestoneAsync(Guid milestoneId, Guid userId)`

## NOT Implemented in Phase 3C
- ❌ No real service implementations
