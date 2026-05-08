using FastEndpoints;
using Org.Backend.Features.Friends.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;

namespace Org.Backend.Features.Friends.Endpoints;

public class RejectFriendRequestEndpoint : Endpoint<RejectFriendRequestRequest, ApiResponse<bool>>
{
    private readonly IFriendService _friendService;

    public RejectFriendRequestEndpoint(IFriendService friendService)
    {
        _friendService = friendService;
    }

    public override void Configure()
    {
        Post("/friends/requests/{id}/reject");
        Description(b => b
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<object>>(400)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404));
    }

    public override async Task HandleAsync(RejectFriendRequestRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            await _friendService.RejectFriendRequestAsync(userId, req.Id, ct);
            Response = ApiResponse<bool>.SuccessResponse(true, "Friend request rejected");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse("Failed to reject friend request", new List<string> { ex.Message });
        }
    }
}

public record RejectFriendRequestRequest
{
    public Guid Id { get; init; }
}

