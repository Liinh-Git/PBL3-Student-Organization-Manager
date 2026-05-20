using Org.Backend.Domain.Entities;
using Org.Shared.Features.Invitations;

namespace Org.Backend.Features.Invitations.Services;

public interface IInvitationService
{
    Task<InvitationDto> CreateInvitationAsync(Guid orgId, Guid inviterUserId, CreateInvitationRequest request, CancellationToken ct = default);
    Task<Request> CreateRecommendationAsync(Guid orgId, Guid recommenderUserId, CreateInvitationRecommendationRequest request, CancellationToken ct = default);
    Task<InvitationDto> CreateInvitationByMemberIdAsync(Guid orgId, Guid inviterMemberId, Guid receiverUserId, string? message, CancellationToken ct = default);
    Task<List<InvitationDto>> GetMyInvitationsAsync(Guid userId, CancellationToken ct = default);
    Task<InvitationDto> AcceptInvitationAsync(Guid userId, Guid invitationId, CancellationToken ct = default);
    Task<InvitationDto> RejectInvitationAsync(Guid userId, Guid invitationId, CancellationToken ct = default);
}
