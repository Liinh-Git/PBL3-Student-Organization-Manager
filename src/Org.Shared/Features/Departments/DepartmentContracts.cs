namespace Org.Shared.Features.Departments;

public sealed record DepartmentDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string? Description,
    Guid? ManagerMemberId,
    int MemberCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record GetDepartmentsRequest(Guid OrganizationId);

public sealed record GetDepartmentsResponse(IReadOnlyList<DepartmentDto> Items);

public sealed record CreateDepartmentRequest(
    Guid OrganizationId,
    string Code,
    string Name,
    string? Description,
    Guid? ManagerMemberId);

public sealed record UpdateDepartmentRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    Guid? ManagerMemberId);
