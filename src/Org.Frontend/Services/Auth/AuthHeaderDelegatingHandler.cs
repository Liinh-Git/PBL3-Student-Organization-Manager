// ---- DelegatingHandler tự động gắn Bearer token vào mọi request API ----
// Ưu tiên đọc từ AccessTokenStore (memory) trước, fallback sang localStorage nếu chưa có.
// Nếu token hết hạn hoặc server trả 401 → xóa token và ném AuthApiException.
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;

namespace Org.Frontend.Services.Auth;

public class AuthHeaderDelegatingHandler : DelegatingHandler
{
    private readonly ITokenStorage _tokenStorage;
    private readonly IAccessTokenStore _accessTokenStore;

    // ---- Inject token storage (localStorage) và in-memory token store ----
    public AuthHeaderDelegatingHandler(ITokenStorage tokenStorage, IAccessTokenStore accessTokenStore)
    {
        _tokenStorage = tokenStorage;
        _accessTokenStore = accessTokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenAttached = false;
        var token = _accessTokenStore.AccessToken;
        var expiresAtUtc = _accessTokenStore.ExpiresAtUtc;

        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                expiresAtUtc = await _tokenStorage.GetTokenExpiryAsync(cancellationToken);
                token = await _tokenStorage.GetTokenAsync(cancellationToken);

                _accessTokenStore.ExpiresAtUtc = expiresAtUtc;
                _accessTokenStore.AccessToken = token;
            }

            if (expiresAtUtc is not null && expiresAtUtc <= DateTime.UtcNow)
            {
                await ClearPersistedTokenAsync(cancellationToken);
                throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                tokenAttached = true;
            }
        }
        catch (InvalidOperationException)
        {
            // Ignore JSInterop exceptions if running during Prerendering
        }
        catch (JSDisconnectedException)
        {
            // Ignore JSInterop exceptions when the SignalR circuit has disconnected.
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (tokenAttached && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await ClearPersistedTokenAsync(cancellationToken);

            response.Dispose();
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);
        }

        return response;
    }

    private async Task ClearPersistedTokenAsync(CancellationToken cancellationToken)
    {
        _accessTokenStore.AccessToken = null;
        _accessTokenStore.ExpiresAtUtc = null;

        try
        {
            await _tokenStorage.ClearAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Ignore JSInterop exceptions if running during Prerendering
        }
        catch (JSDisconnectedException)
        {
            // Ignore JSInterop exceptions when the SignalR circuit has disconnected.
        }
    }
}
