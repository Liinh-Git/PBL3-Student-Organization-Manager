using Org.Backend.Domain.Entities;

namespace Org.Backend.Infrastructure.Auth;

/// <summary>
/// Service for JWT token generation
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate JWT token for authenticated user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <returns>JWT token string and expiration time</returns>
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
