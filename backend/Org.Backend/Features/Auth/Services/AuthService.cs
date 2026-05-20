using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Auth.Mappings;
using Org.Backend.Infrastructure.Auth;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Auth;

namespace Org.Backend.Features.Auth.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        // Normalize email to lowercase for case-insensitive comparison
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, ct);

        // Return generic error if user not found or inactive
        if (user == null || user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Verify password hash using the same hasher as DevDataSeeder
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        // Generate JWT token
        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(user);

        // Return token response
        return new AuthTokenResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAtUtc,
            User = user.ToAuthUserDto()
        };
    }

    public async Task<AuthTokenResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        // Normalize email to lowercase
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Check if email already exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail, ct);

        if (emailExists)
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Create new user entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Status = UserStatus.Active,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        // Hash password using the same hasher as DevDataSeeder
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        // Save user to database
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        // Generate JWT token
        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(user);

        // Return token response
        return new AuthTokenResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAtUtc,
            User = user.ToAuthUserDto()
        };
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        // Find user by ID
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        // Return 404 if user not found
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Return 401 if user is not active
        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException("User account is not active");
        }

        // Return current user response
        return new CurrentUserResponse
        {
            User = user.ToAuthUserDto()
        };
    }
}
