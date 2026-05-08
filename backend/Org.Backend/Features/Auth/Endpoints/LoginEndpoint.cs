using FastEndpoints;
using Org.Backend.Features.Auth.Services;
using Org.Shared.Common;
using Org.Shared.Features.Auth;

namespace Org.Backend.Features.Auth.Endpoints;

/// <summary>
/// Endpoint for user login
/// POST /api/auth/login
/// </summary>
public class LoginEndpoint : Endpoint<LoginRequest, ApiResponse<AuthTokenResponse>>
{
    private readonly IAuthService _authService;

    public LoginEndpoint(IAuthService authService)
    {
        _authService = authService;
    }

    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
        Description(b => b
            .Produces<ApiResponse<AuthTokenResponse>>(200, "application/json")
            .Produces<ApiResponse<AuthTokenResponse>>(400, "application/json")
            .Produces<ApiResponse<AuthTokenResponse>>(401, "application/json")
            .WithTags("Auth"));
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _authService.LoginAsync(req, ct);
            Response = ApiResponse<AuthTokenResponse>.SuccessResponse(result, "Login successful");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 401;
            Response = ApiResponse<AuthTokenResponse>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<AuthTokenResponse>.ErrorResponse("Login failed", new List<string> { ex.Message });
        }
    }
}
