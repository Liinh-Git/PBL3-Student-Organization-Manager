namespace Org.Shared.Features.EventCategories;

/// <summary>
/// Request DTO for creating an event category
/// </summary>
public record CreateEventCategoryRequest
{
    public required string CategoryName { get; init; }
    public string? Description { get; init; }
    public Guid? OwnerDepartmentId { get; init; }
    public int? OrderIndex { get; init; }
}

/// <summary>
/// Request DTO for updating an event category
/// </summary>
public record UpdateEventCategoryRequest
{
    public required string CategoryName { get; init; }
    public string? Description { get; init; }
    public Guid? OwnerDepartmentId { get; init; }
    public int? OrderIndex { get; init; }
}

/// <summary>
/// Response DTO for event category
/// </summary>
public record EventCategoryDto
{
    public required Guid Id { get; init; }
    public required Guid MilestoneId { get; init; }
    public required string CategoryName { get; init; }
    public string? Description { get; init; }
    public Guid? OwnerDepartmentId { get; init; }
    public string? OwnerDepartmentName { get; init; }
    public required int OrderIndex { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public List<Org.Shared.Features.Tasks.TaskDto>? Tasks { get; init; }
}
