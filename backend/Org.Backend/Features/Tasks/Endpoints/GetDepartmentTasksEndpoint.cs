using FastEndpoints;
using Org.Backend.Features.Tasks.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Tasks;

namespace Org.Backend.Features.Tasks.Endpoints;

public class GetDepartmentTasksEndpoint : EndpointWithoutRequest<ApiResponse<List<TaskDto>>>
{
    private readonly ITaskService _taskService;

    public GetDepartmentTasksEndpoint(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public override void Configure()
    {
        Get("/organizations/{orgId}/departments/{departmentId}/tasks");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var orgId = Route<Guid>("orgId");
            var departmentId = Route<Guid>("departmentId");
            var result = await _taskService.GetDepartmentTasksAsync(orgId, departmentId, userId, ct);
            Response = ApiResponse<List<TaskDto>>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<List<TaskDto>>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<List<TaskDto>>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<TaskDto>>.ErrorResponse("Failed to get department tasks", new List<string> { ex.Message });
        }
    }
}

