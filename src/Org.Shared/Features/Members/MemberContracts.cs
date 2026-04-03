namespace Org.Shared.Features.Members;

public enum MemberRole
{
    Member = 0,
    Manager = 1,
    VicePresident = 2,
    President = 3
}

public sealed record MemberDto(
    Guid Id,
    Guid OrganizationId,
    Guid? DepartmentId,
    string StudentCode,
    string FullName,
    string Email,
    MemberRole Role,
    bool IsActive,
    DateTimeOffset JoinedAtUtc);

public sealed record GetMembersRequest(Guid OrganizationId);

public sealed record GetMembersResponse(IReadOnlyList<MemberDto> Items);

public sealed record UpdateMemberRoleRequest(MemberRole Role);

public sealed record UpdateMemberDepartmentRequest(Guid? DepartmentId);
