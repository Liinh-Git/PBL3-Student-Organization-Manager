using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.EventCategories.Services;
using Org.Shared.Common;

namespace Org.Backend.Features.EventCategories.Endpoints;

public class DeleteEventCategoryEndpoint : EndpointWithoutRequest<ApiResponse<bool>>
{
    private readonly IEventCategoryService _categoryService;

    public DeleteEventCategoryEndpoint(IEventCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public override void Configure()
    {
        Delete("/categories/{id}");
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
                Response = ApiResponse<bool>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var idStr = Route<string>("id");
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var categoryId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<bool>.ErrorResponse("Invalid category ID");
                return;
            }

            await _categoryService.DeleteCategoryAsync(categoryId, userId, ct);
            Response = ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully");
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
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse("Failed to delete category", new List<string> { ex.Message });
        }
    }
}
