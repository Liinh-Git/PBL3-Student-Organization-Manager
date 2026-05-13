using FastEndpoints;
using Org.Backend.Features.Invitations.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Invitations;

namespace Org.Backend.Features.Invitations.Endpoints;

public class GetMyInvitationsEndpoint : EndpointWithoutRequest<ApiResponse<List<InvitationDto>>>
{
    private readonly IInvitationService _invitationService;

    public GetMyInvitationsEndpoint(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    public override void Configure()
    {
        Get("/users/me/invitations");
        Description(d => d
            .Produces<ApiResponse<List<InvitationDto>>>(200)
            .Produces<ApiResponse<object>>(400));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _invitationService.GetMyInvitationsAsync(userId, ct);
            Response = ApiResponse<List<InvitationDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<InvitationDto>>.ErrorResponse("Failed to get invitations", new List<string> { ex.Message });
        }
    }
}
