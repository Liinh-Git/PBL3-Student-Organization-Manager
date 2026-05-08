using FastEndpoints;
using Org.Backend.Features.Friends.Services;
using Org.Backend.Features.Friends.Validators;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Friends;

namespace Org.Backend.Features.Friends.Endpoints;

public class SendFriendRequestEndpoint : Endpoint<SendFriendRequestRequest, ApiResponse<FriendRequestDto>>
{
    private readonly IFriendService _friendService;

    public SendFriendRequestEndpoint(IFriendService friendService)
    {
        _friendService = friendService;
    }

    public override void Configure()
    {
        Post("/friends/requests");
        Validator<SendFriendRequestRequestValidator>();
        Description(b => b
            .Produces<ApiResponse<FriendRequestDto>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(404));
    }

    public override async Task HandleAsync(SendFriendRequestRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _friendService.SendFriendRequestAsync(userId, req, ct);
            Response = ApiResponse<FriendRequestDto>.SuccessResponse(result, "Friend request sent successfully");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<FriendRequestDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<FriendRequestDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<FriendRequestDto>.ErrorResponse("Failed to send friend request", new List<string> { ex.Message });
        }
    }
}

