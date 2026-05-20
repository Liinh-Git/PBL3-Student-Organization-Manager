namespace Org.Shared.Features.Auth;

// ============================================================
// REQUEST DTOs
// ============================================================

/// <summary>
/// Request DTO for user login
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// User email address (required, email format)
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User password (required, min 8 characters)
    /// </summary>
    public required string Password { get; init; }
}

/// <summary>
/// Request DTO for user registration
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// User full name (required, max 100 characters)
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// User email address (required, email format, unique)
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User password (required, min 8 characters, strength rules)
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Confirm password (optional, client-side validation only)
    /// </summary>
    public string? ConfirmPassword { get; init; }
}

// ============================================================
// RESPONSE DTOs
// ============================================================

/// <summary>
/// Minimal user info for auth context
/// </summary>
public record AuthUserDto
{
    /// <summary>
    /// User ID (UUID)
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// User full name
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// User email address
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User status (Active, Inactive, Suspended)
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// User avatar URL (optional)
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Last login timestamp (UTC, optional)
    /// </summary>
    public DateTime? LastLoginAtUtc { get; init; }
}

/// <summary>
/// Response DTO for login/register with JWT token
/// </summary>
public record AuthTokenResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Token type (usually "Bearer")
    /// </summary>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Token expiration time (UTC)
    /// </summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>
    /// User info
    /// </summary>
    public required AuthUserDto User { get; init; }
}

/// <summary>
/// Response DTO for current user info
/// </summary>
public record CurrentUserResponse
{
    /// <summary>
    /// User info
    /// </summary>
    public required AuthUserDto User { get; init; }
}
