namespace Org.Shared.Features.Events;

public sealed record EventDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    EventStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record EventTreeNodeDto(
    Guid Id,
    string Name,
    EventStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    int MilestoneCount,
    int CategoryCount,
    int TaskCount,
    int CompletedTaskCount);

public sealed record GetOrganizationEventsRequest(Guid OrganizationId);

public sealed record GetOrganizationEventsResponse(IReadOnlyList<EventTreeNodeDto> Items);

public sealed record GetEventByIdResponse(EventDto Data);

public sealed record CreateEventRequest(
    Guid OrganizationId,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate);
