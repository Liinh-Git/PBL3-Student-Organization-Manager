// ---- Tạo JWT access token từ thông tin user và cấu hình JWT ----
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.Backend.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Org.Backend.Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    // ---- Lấy cấu hình JWT từ dependency injection ----
    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    // ---- Tạo danh sách claims, ký token, và trả về token + hạn sử dụng ----
    public (string token, DateTime expiresAtUtc) CreateAccessToken(User user)
    {
        // Bước 1: xác định thời điểm hiện tại và hạn hết hạn token
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        // Bước 2: dựng danh sách claim sẽ nhúng vào JWT
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("status", user.Status.ToString())
        };

        // Bước 3: tạo khóa ký và thông tin thuật toán ký
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // Bước 4: tạo JWT token object với issuer/audience/claims
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        // Bước 5: serialize token thành chuỗi để trả cho client
        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return (serialized, expiresAt);
    }
}
