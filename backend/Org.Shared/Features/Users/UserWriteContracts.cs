namespace Org.Shared.Features.Users;

// ============================================================
// REQUEST DTOs - User Write Operations
// ============================================================

/// <summary>
/// Request DTO for updating user profile
/// </summary>
public record UpdateUserProfileRequest
{
    /// <summary>
    /// User full name (required, 2-100 characters)
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// Phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Date of birth (optional)
    /// </summary>
    public DateTime? Dob { get; init; }

    /// <summary>
    /// Gender (optional)
    /// </summary>
    public string? Gender { get; init; }

    /// <summary>
    /// Address (optional)
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Avatar URL (optional)
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Bio (optional, max 500 characters)
    /// </summary>
    public string? Bio { get; init; }

    /// <summary>
    /// Social links JSON (optional)
    /// </summary>
    public string? SocialLinks { get; init; }

    /// <summary>
    /// Profile visibility (Public, FriendsOnly, Private)
    /// </summary>
    public string? ProfileVisibility { get; init; }
}

/// <summary>
/// Request DTO for changing password
/// </summary>
public record ChangePasswordRequest
{
    /// <summary>
    /// Current password (required)
    /// </summary>
    public required string CurrentPassword { get; init; }

    /// <summary>
    /// New password (required, min 8 characters)
    /// </summary>
    public required string NewPassword { get; init; }

    /// <summary>
    /// Confirm new password (optional, client-side validation)
    /// </summary>
    public string? ConfirmPassword { get; init; }
}
