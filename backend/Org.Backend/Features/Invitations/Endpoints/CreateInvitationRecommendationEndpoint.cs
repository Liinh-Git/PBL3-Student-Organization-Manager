using FastEndpoints;
using Org.Backend.Features.Invitations.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Invitations;
using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Invitations.Endpoints;

public class CreateInvitationRecommendationEndpoint : Endpoint<CreateInvitationRecommendationRequest, ApiResponse<RequestDto>>
{
    private readonly IInvitationService _invitationService;
    private readonly Org.Backend.Features.Requests.Services.IRequestService _requestService;

    public CreateInvitationRecommendationEndpoint(
        IInvitationService invitationService,
        Org.Backend.Features.Requests.Services.IRequestService requestService)
    {
        _invitationService = invitationService;
        _requestService = requestService;
    }

    public override void Configure()
    {
        Post("/organizations/{orgId}/invitations/recommendations");
        Description(d => d
            .Produces<ApiResponse<RequestDto>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404));
    }

    public override async Task HandleAsync(CreateInvitationRecommendationRequest req, CancellationToken ct)
    {
        try
        {
            var orgId = Route<Guid>("orgId");
            var userId = User.GetUserId();
            var recommendation = await _invitationService.CreateRecommendationAsync(orgId, userId, req, ct);
            var dto = await _requestService.GetRequestByIdAsync(userId, recommendation.Id, ct);
            Response = ApiResponse<RequestDto>.SuccessResponse(dto, "Recommendation submitted for leader review");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<RequestDto>.ErrorResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<RequestDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<RequestDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<RequestDto>.ErrorResponse("Failed to create recommendation", new List<string> { ex.Message });
        }
    }
}
