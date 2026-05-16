using System.Security.Claims;
using FastEndpoints;
using Microsoft.Extensions.Hosting;
using Org.Shared.Common;

namespace Org.Backend.Features.Events.Endpoints;

/// <summary>
/// POST /api/event-banners or /api/events/upload-banner
/// multipart/form-data: file
/// Saves an event banner file and returns the relative URL to store in Event.BannerUrl.
/// </summary>
public class UploadEventBannerEndpoint : EndpointWithoutRequest<ApiResponse<string>>
{
    private const long MaxFileSize = 50 * 1024 * 1024;
    private readonly IHostEnvironment _hostEnvironment;

    public UploadEventBannerEndpoint(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public override void Configure()
    {
        Post("/event-banners", "/events/upload-banner");
        AllowFileUploads();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out _))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<string>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var form = await HttpContext.Request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file");

            if (file == null || file.Length == 0)
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<string>.ErrorResponse("File is required");
                return;
            }

            if (file.Length > MaxFileSize)
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<string>.ErrorResponse("File size must be <= 50MB");
                return;
            }

            var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedTypes.Contains(file.ContentType))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<string>.ErrorResponse("Only jpeg, png, webp images are allowed");
                return;
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = file.ContentType.ToLowerInvariant() switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    _ => ".bin"
                };
            }

            var fileName = $"{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
            var uploadsRoot = Path.Combine(_hostEnvironment.ContentRootPath, "uploads", "events");
            Directory.CreateDirectory(uploadsRoot);
            var absolutePath = Path.Combine(uploadsRoot, fileName);

            await using (var output = File.Create(absolutePath))
            await using (var input = file.OpenReadStream())
            {
                await input.CopyToAsync(output, ct);
            }

            var relativeUrl = $"/uploads/events/{fileName}";
            Response = ApiResponse<string>.SuccessResponse(relativeUrl, "Event banner uploaded successfully");
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<string>.ErrorResponse(ex.Message);
        }
    }
}
