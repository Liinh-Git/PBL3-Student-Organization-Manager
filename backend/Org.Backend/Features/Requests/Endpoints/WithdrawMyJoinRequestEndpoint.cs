using FastEndpoints;
using Org.Backend.Features.Requests.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;

namespace Org.Backend.Features.Requests.Endpoints;

public class WithdrawMyJoinRequestEndpoint : EndpointWithoutRequest<ApiResponse<bool>>
{
    private readonly IRequestService _requestService;

    public WithdrawMyJoinRequestEndpoint(IRequestService requestService)
    {
        _requestService = requestService;
    }

    public override void Configure()
    {
        Post("/organizations/{orgId}/requests/withdraw");
        Description(b => b
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<object>>(401));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var orgId = Route<Guid>("orgId");
            var result = await _requestService.WithdrawMyPendingJoinRequestAsync(userId, orgId, ct);
            Response = ApiResponse<bool>.SuccessResponse(result, result ? "Join request withdrawn" : "No pending join request to withdraw");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 401;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse("Failed to withdraw join request", new List<string> { ex.Message });
        }
    }
}
