using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Tasks.Services;
using Org.Shared.Common;
using Org.Shared.Features.Tasks;

namespace Org.Backend.Features.Tasks.Endpoints;

public class CreateTaskEndpoint : Endpoint<CreateTaskRequest, ApiResponse<TaskDto>>
{
    private readonly ITaskService _taskService;

    public CreateTaskEndpoint(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public override void Configure()
    {
        Post("/categories/{categoryId}/tasks");
        Description(b => b.WithTags("Tasks"));
    }

    public override async Task HandleAsync(CreateTaskRequest req, CancellationToken ct)
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

            var categoryIdStr = Route<string>("categoryId");
            if (string.IsNullOrEmpty(categoryIdStr) || !Guid.TryParse(categoryIdStr, out var categoryId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<TaskDto>.ErrorResponse("Invalid category ID");
                return;
            }

            var result = await _taskService.CreateTaskAsync(categoryId, req, userId, ct);
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
            Response = ApiResponse<TaskDto>.ErrorResponse("Failed to create task", new List<string> { ex.Message });
        }
    }
}
