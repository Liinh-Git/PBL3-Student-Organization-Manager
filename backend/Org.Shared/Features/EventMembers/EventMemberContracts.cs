namespace Org.Shared.Features.EventMembers;

public record EventMemberDto
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required Guid MemberId { get; init; }
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public required DateTime AssignedAtUtc { get; init; }
}

public record AddEventMembersRequest
{
    public required List<Guid> MemberIds { get; init; }
}

