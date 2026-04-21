// ---- Service mock cho xác thực — dùng khi FrontendData:UseMockServices = true ----
// Token mock format: "mock-token:{userId:N}" (không phải JWT thật).
// Dữ liệu user lưu trong FrontendMockDataStore (singleton, load từ users.mock.json).
using Org.Frontend.Services.Auth;
using Org.Frontend.Services.Mocks.Models;
using Org.Shared.Features.Auth;

namespace Org.Frontend.Services.Mocks;

public sealed class AuthMockService(FrontendMockDataStore mockDataStore) : IAuthService
{
    private const string MockTokenPrefix = "mock-token:";
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;

    public Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var fullName = NormalizeRequired(request.FullName, "Họ và tên là bắt buộc.");
            var email = NormalizeEmail(request.Email);
            ValidatePassword(request.Password);

            if (data.Users.Any(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new AuthApiException("Email này đã được đăng ký.", 409);
            }

            var user = new MockUser
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email
            };

            data.Users.Add(user);

            return new RegisterResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };
        }, ct);
    }

    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var email = NormalizeEmail(request.Email);
            ValidatePassword(request.Password);

            var user = data.Users.FirstOrDefault(x =>
                string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));

            if (user is null)
            {
                throw new AuthApiException("Email hoặc mật khẩu không đúng.", 401);
            }

            return new LoginResponse
            {
                AccessToken = BuildToken(user.Id),
                ExpiresAtUtc = DateTime.UtcNow.AddHours(12),
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };
        }, ct);
    }

    public Task<MeResponse> GetMeAsync(string accessToken, CancellationToken ct = default)
    {
        return _mockDataStore.UseAsync(data =>
        {
            var userId = ParseTokenUserId(accessToken);
            var user = data.Users.FirstOrDefault(x => x.Id == userId);

            if (user is null)
            {
                throw new AuthApiException("Phiên đăng nhập không hợp lệ.", 401);
            }

            return new MeResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Status = "Active"
            };
        }, ct);
    }

    private static string BuildToken(Guid userId)
        => $"{MockTokenPrefix}{userId:N}";

    private static Guid ParseTokenUserId(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken) ||
            !accessToken.StartsWith(MockTokenPrefix, StringComparison.Ordinal))
        {
            throw new AuthApiException("Phiên đăng nhập không hợp lệ.", 401);
        }

        var rawUserId = accessToken[MockTokenPrefix.Length..];
        if (!Guid.TryParseExact(rawUserId, "N", out var userId))
        {
            throw new AuthApiException("Phiên đăng nhập không hợp lệ.", 401);
        }

        return userId;
    }

    private static string NormalizeRequired(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AuthApiException(message, 400);
        }

        return value.Trim();
    }

    private static string NormalizeEmail(string? email)
    {
        var normalized = NormalizeRequired(email, "Email là bắt buộc.");
        if (!normalized.Contains('@'))
        {
            throw new AuthApiException("Email không hợp lệ.", 400);
        }

        return normalized;
    }

    private static void ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw new AuthApiException("Mật khẩu phải có tối thiểu 8 ký tự.", 400);
        }
    }
}
