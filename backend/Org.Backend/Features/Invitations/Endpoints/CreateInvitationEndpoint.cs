using FastEndpoints;
using Org.Backend.Features.Invitations.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Invitations;

namespace Org.Backend.Features.Invitations.Endpoints;

public class CreateInvitationEndpoint : Endpoint<CreateInvitationRequest, ApiResponse<InvitationDto>>
{
    private readonly IInvitationService _invitationService;

    public CreateInvitationEndpoint(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    public override void Configure()
    {
        Post("/organizations/{orgId}/invitations");
        Description(d => d
            .Produces<ApiResponse<InvitationDto>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404));
    }

    public override async Task HandleAsync(CreateInvitationRequest req, CancellationToken ct)
    {
        try
        {
            var orgId = Route<Guid>("orgId");
            var userId = User.GetUserId();
            var result = await _invitationService.CreateInvitationAsync(orgId, userId, req, ct);
            Response = ApiResponse<InvitationDto>.SuccessResponse(result, "Invitation sent successfully");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<InvitationDto>.ErrorResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
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
            Response = ApiResponse<InvitationDto>.ErrorResponse("Failed to send invitation", new List<string> { ex.Message });
        }
    }
}
