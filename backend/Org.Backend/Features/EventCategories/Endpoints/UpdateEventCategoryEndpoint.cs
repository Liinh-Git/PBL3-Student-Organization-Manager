using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.EventCategories.Services;
using Org.Shared.Common;
using Org.Shared.Features.EventCategories;

namespace Org.Backend.Features.EventCategories.Endpoints;

public class UpdateEventCategoryEndpoint : Endpoint<UpdateEventCategoryRequest, ApiResponse<EventCategoryDto>>
{
    private readonly IEventCategoryService _categoryService;

    public UpdateEventCategoryEndpoint(IEventCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public override void Configure()
    {
        Put("/categories/{id}");
        Description(b => b.WithTags("EventCategories"));
    }

    public override async Task HandleAsync(UpdateEventCategoryRequest req, CancellationToken ct)
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

            var idStr = Route<string>("id");
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var categoryId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<EventCategoryDto>.ErrorResponse("Invalid category ID");
                return;
            }

            var result = await _categoryService.UpdateCategoryAsync(categoryId, req, userId, ct);
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
            Response = ApiResponse<EventCategoryDto>.ErrorResponse("Failed to update category", new List<string> { ex.Message });
        }
    }
}
