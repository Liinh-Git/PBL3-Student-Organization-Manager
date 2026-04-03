namespace Org.Backend.Features.Milestones;

internal static class MilestonesFeatureTodos
{
    // TODO(BE-DAY1): POST /api/events/{eventId}/milestones
    //  - Validate event exists and eventId matches body.EventId.
    //  - Validate milestone date range inside event date range.
    //  - Return 201 with created MilestoneDto.
    //
    // TODO(BE-DAY1): GET /api/events/{eventId}/milestones
    //  - Return 200 with { items: [...] } ordered by SortOrder.
    //  - Return 404 if event not found.
}
