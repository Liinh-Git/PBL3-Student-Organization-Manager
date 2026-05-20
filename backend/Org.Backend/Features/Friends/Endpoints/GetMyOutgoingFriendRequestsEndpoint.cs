using FastEndpoints;
using Org.Backend.Features.Friends.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Friends;

namespace Org.Backend.Features.Friends.Endpoints;

public class GetMyOutgoingFriendRequestsEndpoint : EndpointWithoutRequest<ApiResponse<List<FriendRequestDto>>>
{
    private readonly IFriendService _friendService;

    public GetMyOutgoingFriendRequestsEndpoint(IFriendService friendService)
    {
        _friendService = friendService;
    }

    public override void Configure()
    {
        Get("/friends/requests/outgoing");
        Description(b => b
            .Produces<ApiResponse<List<FriendRequestDto>>>(200)
            .Produces<ApiResponse<object>>(400));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _friendService.GetMyOutgoingFriendRequestsAsync(userId, ct);
            Response = ApiResponse<List<FriendRequestDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<FriendRequestDto>>.ErrorResponse("Failed to get outgoing friend requests", new List<string> { ex.Message });
        }
    }
}
