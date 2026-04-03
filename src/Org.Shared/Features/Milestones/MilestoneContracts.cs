namespace Org.Shared.Features.Milestones;

public sealed record MilestoneDto(
    Guid Id,
    Guid EventId,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    int SortOrder,
    string Status);

public sealed record CreateMilestoneRequest(
    Guid EventId,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    int SortOrder);

public sealed record GetMilestonesResponse(IReadOnlyList<MilestoneDto> Items);
