using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Tasks.Services;
using Org.Shared.Common;
using Org.Shared.Features.Tasks;

namespace Org.Backend.Features.Tasks.Endpoints;

public class AssignTaskEndpoint : Endpoint<AssignTaskRequest, ApiResponse<TaskDto>>
{
    private readonly ITaskService _taskService;

    public AssignTaskEndpoint(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public override void Configure()
    {
        Put("/tasks/{taskId}/assign");
        Description(b => b.WithTags("Tasks"));
    }

    public override async Task HandleAsync(AssignTaskRequest req, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<TaskDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var taskIdStr = Route<string>("taskId");
            if (string.IsNullOrEmpty(taskIdStr) || !Guid.TryParse(taskIdStr, out var taskId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<TaskDto>.ErrorResponse("Invalid task ID");
                return;
            }

            var result = await _taskService.AssignTaskAsync(taskId, req, userId, ct);
            Response = ApiResponse<TaskDto>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<TaskDto>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<TaskDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<TaskDto>.ErrorResponse("Failed to assign task", new List<string> { ex.Message });
        }
    }
}
