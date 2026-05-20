using FastEndpoints;
using Org.Backend.Features.Friends.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Friends;

namespace Org.Backend.Features.Friends.Endpoints;

public class GetFriendSuggestionsEndpoint : EndpointWithoutRequest<ApiResponse<List<FriendDto>>>
{
    private readonly IFriendService _friendService;

    public GetFriendSuggestionsEndpoint(IFriendService friendService)
    {
        _friendService = friendService;
    }

    public override void Configure()
    {
        Get("/friends/suggestions");
        Description(b => b
            .Produces<ApiResponse<List<FriendDto>>>(200)
            .Produces<ApiResponse<object>>(400));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _friendService.GetFriendSuggestionsAsync(userId, ct);
            Response = ApiResponse<List<FriendDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<FriendDto>>.ErrorResponse("Failed to get friend suggestions", new List<string> { ex.Message });
        }
    }
}
