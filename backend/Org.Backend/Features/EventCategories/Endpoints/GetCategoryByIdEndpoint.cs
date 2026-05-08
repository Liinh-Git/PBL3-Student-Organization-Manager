using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.EventCategories.Services;
using Org.Shared.Common;
using Org.Shared.Features.EventCategories;

namespace Org.Backend.Features.EventCategories.Endpoints;

public class GetCategoryByIdEndpoint : EndpointWithoutRequest<ApiResponse<EventCategoryDto>>
{
    private readonly IEventCategoryService _categoryService;

    public GetCategoryByIdEndpoint(IEventCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public override void Configure()
    {
        Get("/categories/{id}");
        Description(b => b.WithTags("EventCategories"));
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

            var result = await _categoryService.GetCategoryByIdAsync(categoryId, userId, ct);
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
            Response = ApiResponse<EventCategoryDto>.ErrorResponse("Failed to get category", new List<string> { ex.Message });
        }
    }
}
