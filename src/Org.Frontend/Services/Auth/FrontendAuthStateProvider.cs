// ---- Quản lý auth state cho Blazor: khởi tạo từ token, sign in, sign out ----
using Microsoft.AspNetCore.Components.Authorization;
using Org.Shared.Features.Auth;
using System.Security.Claims;

namespace Org.Frontend.Services.Auth;

public sealed class FrontendAuthStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState AnonymousState = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly AuthApiClient _authApiClient;
    private readonly ITokenStorage _tokenStorage;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private AuthenticationState _currentState = AnonymousState;
    private bool _initialized;

    // ---- Inject API client và token storage ----
    public FrontendAuthStateProvider(AuthApiClient authApiClient, ITokenStorage tokenStorage)
    {
        _authApiClient = authApiClient;
        _tokenStorage = tokenStorage;
    }

    // ---- Trả về auth state hiện tại cho UI ----
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(_currentState);
    }

    // ---- Khởi tạo: đọc token, kiểm tra hết hạn, gọi /me nếu cần ----
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        // Bỏ qua nếu state đã được khởi tạo trước đó
        if (_initialized)
            return;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_initialized)
                return;

            // Bước 1: đọc token và thời hạn từ storage
            var token = await _tokenStorage.GetTokenAsync(ct);
            var expiresAtUtc = await _tokenStorage.GetTokenExpiryAsync(ct);

            // Bước 2: token thiếu/hết hạn thì clear và set anonymous
            if (string.IsNullOrWhiteSpace(token) || expiresAtUtc is null || expiresAtUtc <= DateTime.UtcNow)
            {
                await _tokenStorage.ClearAsync(ct);
                SetAnonymousState();
                _initialized = true;
                return;
            }

            try
            {
                // Bước 3: token hợp lệ thì gọi /me để dựng claims chuẩn
                var me = await _authApiClient.GetMeAsync(token, ct);
                SetAuthenticatedState(BuildClaimsPrincipal(me));
            }
            catch (AuthApiException)
            {
                // Bước 4: token không dùng được thì fallback về anonymous
                await _tokenStorage.ClearAsync(ct);
                SetAnonymousState();
            }

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    // ---- Khi login thành công: lưu token và set auth state ----
    public async Task SignInAsync(LoginResponse loginResponse, CancellationToken ct = default)
    {
        // Bước 1: lưu token + hạn sử dụng để giữ phiên đăng nhập
        await _tokenStorage.SaveTokenAsync(loginResponse.AccessToken, loginResponse.ExpiresAtUtc, ct);

        MeResponse profile;
        try
        {
            // Bước 2: ưu tiên gọi /me để lấy profile mới nhất từ backend
            profile = await _authApiClient.GetMeAsync(loginResponse.AccessToken, ct);
        }
        catch (AuthApiException)
        {
            // Bước 2b: fallback dùng dữ liệu có sẵn trong login response
            profile = new MeResponse
            {
                UserId = loginResponse.UserId,
                FullName = loginResponse.FullName,
                Email = loginResponse.Email,
                Status = "Active"
            };
        }

        SetAuthenticatedState(BuildClaimsPrincipal(profile));
        _initialized = true;
    }

    // ---- Đăng xuất: xóa token và reset state ----
    public async Task SignOutAsync(CancellationToken ct = default)
    {
        // Xóa dữ liệu phiên local rồi phát thông báo state mới cho UI
        await _tokenStorage.ClearAsync(ct);
        SetAnonymousState();
        _initialized = true;
    }

    // ---- Tạo ClaimsPrincipal từ profile của user ----
    private static ClaimsPrincipal BuildClaimsPrincipal(MeResponse profile)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, profile.UserId.ToString()),
            new(ClaimTypes.Name, profile.FullName),
            new(ClaimTypes.Email, profile.Email),
            new("status", profile.Status)
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "Bearer");
        return new ClaimsPrincipal(identity);
    }

    // ---- Cập nhật state sang authenticated và thông báo UI ----
    private void SetAuthenticatedState(ClaimsPrincipal principal)
    {
        _currentState = new AuthenticationState(principal);
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    // ---- Cập nhật state sang anonymous và thông báo UI ----
    private void SetAnonymousState()
    {
        _currentState = AnonymousState;
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }
}
