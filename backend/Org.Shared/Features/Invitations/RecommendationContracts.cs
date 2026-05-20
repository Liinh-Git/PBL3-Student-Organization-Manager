namespace Org.Shared.Features.Invitations;

public record CreateInvitationRecommendationRequest
{
    public required Guid ReceiverUserId { get; init; }
    public string? Message { get; init; }
}
