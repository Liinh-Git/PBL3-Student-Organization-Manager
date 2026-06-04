using FastEndpoints;
using Org.Backend.Features.Tasks.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Tasks;

namespace Org.Backend.Features.Tasks.Endpoints;

public class CreateDepartmentTaskEndpoint : Endpoint<CreateDepartmentTaskEndpointRequest, ApiResponse<TaskDto>>
{
    private readonly ITaskService _taskService;

    public CreateDepartmentTaskEndpoint(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public override void Configure()
    {
        Post("/organizations/{orgId}/departments/{departmentId}/tasks");
    }

    public override async Task HandleAsync(CreateDepartmentTaskEndpointRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var orgId = Route<Guid>("orgId");
            var departmentId = Route<Guid>("departmentId");

            var result = await _taskService.CreateDepartmentTaskAsync(
                orgId,
                departmentId,
                new CreateDepartmentTaskRequest
                {
                    CategoryId = req.CategoryId,
                    Task = req.Task
                },
                userId,
                ct);

            Response = ApiResponse<TaskDto>.SuccessResponse(result, "Department task created successfully");
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
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<TaskDto>.ErrorResponse(ex.Message);
        }
        catch (ArgumentException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<TaskDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<TaskDto>.ErrorResponse("Failed to create department task", new List<string> { ex.Message });
        }
    }
}

public record CreateDepartmentTaskEndpointRequest
{
    public Guid? CategoryId { get; init; }
    public CreateTaskRequest Task { get; init; } = null!;
}
