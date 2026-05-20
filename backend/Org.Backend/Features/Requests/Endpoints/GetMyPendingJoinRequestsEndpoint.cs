using FastEndpoints;
using Org.Backend.Features.Requests.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Requests;

namespace Org.Backend.Features.Requests.Endpoints;

public class GetMyPendingJoinRequestsEndpoint : EndpointWithoutRequest<ApiResponse<List<MyPendingJoinRequestDto>>>
{
    private readonly IRequestService _requestService;

    public GetMyPendingJoinRequestsEndpoint(IRequestService requestService)
    {
        _requestService = requestService;
    }

    public override void Configure()
    {
        Get("/users/me/requests/pending-join");
        Description(b => b
            .Produces<ApiResponse<List<MyPendingJoinRequestDto>>>(200)
            .Produces<ApiResponse<object>>(401));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _requestService.GetMyPendingJoinRequestsAsync(userId, ct);
            Response = ApiResponse<List<MyPendingJoinRequestDto>>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 401;
            Response = ApiResponse<List<MyPendingJoinRequestDto>>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<MyPendingJoinRequestDto>>.ErrorResponse("Failed to load pending join requests", new List<string> { ex.Message });
        }
    }
}

