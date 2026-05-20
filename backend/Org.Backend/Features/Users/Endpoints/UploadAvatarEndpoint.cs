using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Org.Backend.Features.Users.Services;
using Org.Shared.Common;
using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Endpoints;

/// <summary>
/// POST /api/users/me/avatar
/// multipart/form-data: file
/// </summary>
public class UploadAvatarEndpoint : EndpointWithoutRequest<ApiResponse<UserProfileDto>>
{
    private const long MaxFileSize = 20 * 1024 * 1024;
    private readonly IUserService _userService;

    public UploadAvatarEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Post("/users/me/avatar");
        AllowFileUploads();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<UserProfileDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var form = await HttpContext.Request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file");

            if (file == null || file.Length == 0)
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<UserProfileDto>.ErrorResponse("File is required");
                return;
            }

            if (file.Length > MaxFileSize)
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<UserProfileDto>.ErrorResponse("File size must be <= 20MB");
                return;
            }

            await using var stream = file.OpenReadStream();
            var userProfile = await _userService.UploadAvatarAsync(
                userId,
                stream,
                file.FileName,
                file.ContentType,
                ct);

            Response = ApiResponse<UserProfileDto>.SuccessResponse(userProfile, "Avatar uploaded successfully");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<UserProfileDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<UserProfileDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<UserProfileDto>.ErrorResponse(ex.Message);
        }
    }
}
