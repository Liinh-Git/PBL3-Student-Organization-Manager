using Org.Shared.Features.Auth;

namespace Org.Backend.Features.Auth.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user and generate JWT token
    /// </summary>
    Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// Register new user account
    /// </summary>
    Task<AuthTokenResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get current authenticated user info
    /// </summary>
    Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}
