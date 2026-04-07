namespace Org.Shared.Features.EventCategories;

public sealed record EventCategoryDto(
    Guid Id,
    Guid MilestoneId,
    string Name,
    string? Description,
    int SortOrder,
    int TaskCount,
    int CompletedTaskCount);

public sealed record CreateEventCategoryRequest(
    Guid MilestoneId,
    string Name,
    string? Description,
    int SortOrder);

public sealed record GetEventCategoriesResponse(IReadOnlyList<EventCategoryDto> Items);
