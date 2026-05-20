using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Tasks.Services;
using Org.Shared.Common;

namespace Org.Backend.Features.Tasks.Endpoints;

public class DeleteTaskEndpoint : EndpointWithoutRequest<ApiResponse<bool>>
{
    private readonly ITaskService _taskService;

    public DeleteTaskEndpoint(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public override void Configure()
    {
        Delete("/tasks/{taskId}");
        Description(b => b.WithTags("Tasks"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<bool>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var taskIdStr = Route<string>("taskId");
            if (string.IsNullOrEmpty(taskIdStr) || !Guid.TryParse(taskIdStr, out var taskId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<bool>.ErrorResponse("Invalid task ID");
                return;
            }

            await _taskService.DeleteTaskAsync(taskId, userId, ct);
            Response = ApiResponse<bool>.SuccessResponse(true, "Task deleted successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse("Failed to delete task", new List<string> { ex.Message });
        }
    }
}
