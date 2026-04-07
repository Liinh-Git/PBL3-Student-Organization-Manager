// ---- Lưu token vào localStorage qua JS interop ----
using Microsoft.JSInterop;
using System.Globalization;

namespace Org.Frontend.Services.Auth;

public sealed class LocalStorageTokenStorage : ITokenStorage
{
    // Key lưu access token trong localStorage
    private const string AccessTokenKey = "org.auth.accessToken";
    // Key lưu thời điểm hết hạn token theo UTC
    private const string AccessTokenExpiryKey = "org.auth.accessTokenExpiryUtc";

    private readonly IJSRuntime _jsRuntime;

    // ---- Inject JS runtime để gọi localStorage ----
    public LocalStorageTokenStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    // ---- Đọc access token từ localStorage ----
    public Task<string?> GetTokenAsync(CancellationToken ct = default)
    {
        return _jsRuntime.InvokeAsync<string?>("localStorage.getItem", ct, AccessTokenKey).AsTask();
    }

    // ---- Đọc thời gian hết hạn và parse về UTC ----
    public async Task<DateTime?> GetTokenExpiryAsync(CancellationToken ct = default)
    {
        var raw = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", ct, AccessTokenExpiryKey);
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        if (!DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsed))
            return null;

        return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
    }

    // ---- Lưu token và thời gian hết hạn (ISO-8601) ----
    public async Task SaveTokenAsync(string token, DateTime expiresAtUtc, CancellationToken ct = default)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ct, AccessTokenKey, token);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ct, AccessTokenExpiryKey, expiresAtUtc.ToString("O", CultureInfo.InvariantCulture));
    }

    // ---- Xóa token khỏi localStorage ----
    public async Task ClearAsync(CancellationToken ct = default)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", ct, AccessTokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", ct, AccessTokenExpiryKey);
    }
}
