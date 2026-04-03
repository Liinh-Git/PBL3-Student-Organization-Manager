// ---- Hợp đồng JWT: tạo access token cho user ----
using Org.Backend.Domain.Entities;

namespace Org.Backend.Infrastructure.Auth;

public interface IJwtTokenService
{
    // ---- Tạo access token và thời gian hết hạn ----
    (string token, DateTime expiresAtUtc) CreateAccessToken(User user);
}
