namespace Org.Shared.Features.Members;

/// <summary>
/// Member DTO for list and detail views
/// </summary>
public record MemberDto
{
    public required Guid Id { get; init; }
    public required Guid OrganizationId { get; init; }
    public required Guid UserId { get; init; }
    public Guid? DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public Guid? RoleId { get; init; }
    public string? RoleName { get; init; }
    public string? StudentCode { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? AvatarUrl { get; init; }
    public required string Status { get; init; }
    public required DateTime JoinedAtUtc { get; init; }
}
