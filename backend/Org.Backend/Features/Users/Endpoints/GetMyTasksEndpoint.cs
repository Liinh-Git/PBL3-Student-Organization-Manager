using FastEndpoints;
using Org.Backend.Features.Users.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Endpoints;

public record GetMyTasksRequest
{
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
}

/// <summary>
/// Endpoint for getting current user's assigned tasks (with event context)
/// GET /api/users/me/tasks
/// </summary>
public class GetMyTasksEndpoint : Endpoint<GetMyTasksRequest, ApiResponse<List<MyTaskDto>>>
{
    private readonly IUserService _userService;

    public GetMyTasksEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/users/me/tasks");
        Description(b => b
            .Produces<ApiResponse<List<MyTaskDto>>>(200, "application/json")
            .Produces<ApiResponse<List<MyTaskDto>>>(401, "application/json")
            .WithTags("Users"));
    }

    public override async Task HandleAsync(GetMyTasksRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _userService.GetMyTasksAsync(userId, req.FromUtc, req.ToUtc, ct);
            Response = ApiResponse<List<MyTaskDto>>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 401;
            Response = ApiResponse<List<MyTaskDto>>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<MyTaskDto>>.ErrorResponse("Failed to get tasks", new List<string> { ex.Message });
        }
    }
}

