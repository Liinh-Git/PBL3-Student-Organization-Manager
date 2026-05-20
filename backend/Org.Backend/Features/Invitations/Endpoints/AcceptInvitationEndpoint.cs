using FastEndpoints;
using Org.Backend.Features.Invitations.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Invitations;

namespace Org.Backend.Features.Invitations.Endpoints;

public class AcceptInvitationEndpoint : Endpoint<AcceptInvitationRequest, ApiResponse<InvitationDto>>
{
    private readonly IInvitationService _invitationService;

    public AcceptInvitationEndpoint(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    public override void Configure()
    {
        Post("/users/me/invitations/{invitationId}/accept");
        Description(d => d
            .Produces<ApiResponse<InvitationDto>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(404));
    }

    public override async Task HandleAsync(AcceptInvitationRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _invitationService.AcceptInvitationAsync(userId, req.InvitationId, ct);
            Response = ApiResponse<InvitationDto>.SuccessResponse(result, "Invitation accepted");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<InvitationDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<InvitationDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<InvitationDto>.ErrorResponse("Failed to accept invitation", new List<string> { ex.Message });
        }
    }
}

public record AcceptInvitationRequest
{
    public Guid InvitationId { get; init; }
}
