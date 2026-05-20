using FastEndpoints;
using Org.Backend.Features.Auth.Services;
using Org.Shared.Common;
using Org.Shared.Features.Auth;

namespace Org.Backend.Features.Auth.Endpoints;

/// <summary>
/// Endpoint for user registration
/// POST /api/auth/register
/// </summary>
public class RegisterEndpoint : Endpoint<RegisterRequest, ApiResponse<AuthTokenResponse>>
{
    private readonly IAuthService _authService;

    public RegisterEndpoint(IAuthService authService)
    {
        _authService = authService;
    }

    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
        Description(b => b
            .Produces<ApiResponse<AuthTokenResponse>>(200, "application/json")
            .Produces<ApiResponse<AuthTokenResponse>>(400, "application/json")
            .Produces<ApiResponse<AuthTokenResponse>>(409, "application/json")
            .WithTags("Auth"));
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _authService.RegisterAsync(req, ct);
            Response = ApiResponse<AuthTokenResponse>.SuccessResponse(result, "Registration successful");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            HttpContext.Response.StatusCode = 409;
            Response = ApiResponse<AuthTokenResponse>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<AuthTokenResponse>.ErrorResponse("Registration failed", new List<string> { ex.Message });
        }
    }
}
