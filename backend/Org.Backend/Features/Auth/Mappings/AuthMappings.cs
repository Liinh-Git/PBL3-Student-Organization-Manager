using Org.Backend.Domain.Entities;
using Org.Shared.Features.Auth;

namespace Org.Backend.Features.Auth.Mappings;

/// <summary>
/// Mapping extensions for Auth module
/// </summary>
public static class AuthMappings
{
    /// <summary>
    /// Map User entity to AuthUserDto
    /// CRITICAL: Never expose PasswordHash
    /// </summary>
    public static AuthUserDto ToAuthUserDto(this User user)
    {
        return new AuthUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Status = user.Status.ToString(),
            AvatarUrl = user.AvatarUrl,
            LastLoginAtUtc = user.LastLoginAt
        };
    }
}
