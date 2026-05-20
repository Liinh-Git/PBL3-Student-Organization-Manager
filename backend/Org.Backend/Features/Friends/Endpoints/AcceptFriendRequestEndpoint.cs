using FastEndpoints;
using Org.Backend.Features.Friends.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Friends;

namespace Org.Backend.Features.Friends.Endpoints;

public class AcceptFriendRequestEndpoint : Endpoint<AcceptFriendRequestRequest, ApiResponse<FriendDto>>
{
    private readonly IFriendService _friendService;

    public AcceptFriendRequestEndpoint(IFriendService friendService)
    {
        _friendService = friendService;
    }

    public override void Configure()
    {
        Post("/friends/requests/{id}/accept");
        Description(b => b
            .Produces<ApiResponse<FriendDto>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404));
    }

    public override async Task HandleAsync(AcceptFriendRequestRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _friendService.AcceptFriendRequestAsync(userId, req.Id, ct);
            Response = ApiResponse<FriendDto>.SuccessResponse(result, "Friend request accepted");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<FriendDto>.ErrorResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<FriendDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<FriendDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<FriendDto>.ErrorResponse("Failed to accept friend request", new List<string> { ex.Message });
        }
    }
}

public record AcceptFriendRequestRequest
{
    public Guid Id { get; init; }
}

