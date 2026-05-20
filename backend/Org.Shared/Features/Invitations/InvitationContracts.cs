namespace Org.Shared.Features.Invitations;

public record CreateInvitationRequest
{
    public required Guid ReceiverUserId { get; init; }
    public string? Message { get; init; }
}

public record InvitationDto
{
    public required Guid InvitationId { get; init; }
    public required Guid OrganizationId { get; init; }
    public required string OrganizationName { get; init; }
    public required Guid ReceiverUserId { get; init; }
    public required Guid InviterUserId { get; init; }
    public required string InviterName { get; init; }
    public required string Status { get; init; }
    public string? Message { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? RespondedAt { get; init; }
}
