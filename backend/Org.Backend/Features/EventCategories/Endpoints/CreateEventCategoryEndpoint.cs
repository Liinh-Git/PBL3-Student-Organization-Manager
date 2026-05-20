using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.EventCategories.Services;
using Org.Shared.Common;
using Org.Shared.Features.EventCategories;

namespace Org.Backend.Features.EventCategories.Endpoints;

public class CreateEventCategoryEndpoint : Endpoint<CreateEventCategoryRequest, ApiResponse<EventCategoryDto>>
{
    private readonly IEventCategoryService _categoryService;

    public CreateEventCategoryEndpoint(IEventCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public override void Configure()
    {
        Post("/milestones/{milestoneId}/categories");
        Description(b => b.WithTags("EventCategories"));
    }

    public override async Task HandleAsync(CreateEventCategoryRequest req, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<EventCategoryDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var milestoneIdStr = Route<string>("milestoneId");
            if (string.IsNullOrEmpty(milestoneIdStr) || !Guid.TryParse(milestoneIdStr, out var milestoneId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<EventCategoryDto>.ErrorResponse("Invalid milestone ID");
                return;
            }

            var result = await _categoryService.CreateCategoryAsync(milestoneId, req, userId, ct);
            Response = ApiResponse<EventCategoryDto>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<EventCategoryDto>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<EventCategoryDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<EventCategoryDto>.ErrorResponse("Failed to create category", new List<string> { ex.Message });
        }
    }
}
