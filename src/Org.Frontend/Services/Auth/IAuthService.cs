// ---- Hợp đồng interface cho service xác thực (Auth) ----
// Được implement bởi: AuthApiClient (thực) và AuthMockService (mock)
using Org.Shared.Features.Auth;

namespace Org.Frontend.Services.Auth;

public interface IAuthService
{
    // ---- Đăng ký tài khoản mới ----
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    // ---- Đăng nhập và lấy access token ----
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    // ---- Lấy thông tin profile user từ access token hiện tại ----
    Task<MeResponse> GetMeAsync(string accessToken, CancellationToken ct = default);
}
