using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Org.Backend.Features.Organizations.Services;
using Org.Shared.Common;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations.Endpoints;

public class UploadOrganizationImageRequest
{
    public IFormFile? File { get; set; }
    public string? Type { get; set; }
}

/// <summary>
/// POST /api/organizations/{id}/upload-image
/// multipart/form-data: file, type(avatar|cover)
/// </summary>
public class UploadOrganizationImageEndpoint : Endpoint<UploadOrganizationImageRequest, ApiResponse<OrganizationDto>>
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly IOrganizationService _organizationService;

    public UploadOrganizationImageEndpoint(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    public override void Configure()
    {
        Post("/organizations/{id}/upload-image");
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadOrganizationImageRequest req, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<OrganizationDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var orgIdStr = Route<string>("id");
            if (string.IsNullOrWhiteSpace(orgIdStr) || !Guid.TryParse(orgIdStr, out var orgId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<OrganizationDto>.ErrorResponse("Invalid organization ID");
                return;
            }

            if (req.File == null || req.File.Length == 0)
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<OrganizationDto>.ErrorResponse("File is required");
                return;
            }

            if (req.File.Length > MaxFileSize)
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<OrganizationDto>.ErrorResponse("File size must be <= 5MB");
                return;
            }

            var type = (req.Type ?? string.Empty).Trim().ToLowerInvariant();
            if (type != "avatar" && type != "cover")
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<OrganizationDto>.ErrorResponse("Type must be avatar or cover");
                return;
            }

            await using var stream = req.File.OpenReadStream();
            var org = await _organizationService.UploadOrganizationImageAsync(
                orgId,
                userId,
                stream,
                req.File.FileName,
                req.File.ContentType,
                type,
                ct);

            Response = ApiResponse<OrganizationDto>.SuccessResponse(org, "Image uploaded successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<OrganizationDto>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<OrganizationDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<OrganizationDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<OrganizationDto>.ErrorResponse(ex.Message);
        }
    }
}
