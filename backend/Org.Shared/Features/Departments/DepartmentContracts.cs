namespace Org.Shared.Features.Departments;

/// <summary>
/// Department DTO for list and detail views
/// </summary>
public record DepartmentDto
{
    public required Guid Id { get; init; }
    public required Guid OrganizationId { get; init; }
    public required string DeptName { get; init; }
    public string? Code { get; init; }
    public string? Function { get; init; }
    public Guid? ManagerId { get; init; }
    public string? ManagerName { get; init; }
    public int MemberCount { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
}
