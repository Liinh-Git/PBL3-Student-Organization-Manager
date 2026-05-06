// ---- DelegatingHandler tự động gắn Bearer token vào mọi request API ----
// Ưu tiên đọc từ AccessTokenStore (memory) trước, fallback sang localStorage nếu chưa có.
// Nếu token hết hạn hoặc server trả 401 → xóa token và ném AuthApiException.
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Org.Frontend.Infrastructure.Auth;

namespace Org.Frontend.Services.Auth;

public class AuthHeaderDelegatingHandler : DelegatingHandler
{
    private readonly ITokenStorage _fallbackTokenStorage;
    private readonly IAccessTokenStore _fallbackAccessTokenStore;
    private readonly CircuitServicesAccessor _circuitServicesAccessor;
    private readonly ILogger<AuthHeaderDelegatingHandler> _logger;

    // ---- Inject token storage (localStorage) và in-memory token store ----
    public AuthHeaderDelegatingHandler(
        ITokenStorage tokenStorage,
        IAccessTokenStore accessTokenStore,
        CircuitServicesAccessor circuitServicesAccessor,
        ILogger<AuthHeaderDelegatingHandler> logger)
    {
        _fallbackTokenStorage = tokenStorage;
        _fallbackAccessTokenStore = accessTokenStore;
        _circuitServicesAccessor = circuitServicesAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenStorage = _fallbackTokenStorage;
        var accessTokenStore = _fallbackAccessTokenStore;
        var tokenSource = "handler-scope";

        if (TryResolveCircuitTokenServices(out var circuitTokenStorage, out var circuitTokenStore))
        {
            tokenStorage = circuitTokenStorage;
            accessTokenStore = circuitTokenStore;
            tokenSource = "circuit-scope";
        }

        var token = accessTokenStore.AccessToken;
        var expiresAtUtc = accessTokenStore.ExpiresAtUtc;
        var authHeaderAttached = false;

        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                expiresAtUtc = await tokenStorage.GetTokenExpiryAsync(cancellationToken);
                token = await tokenStorage.GetTokenAsync(cancellationToken);

                accessTokenStore.ExpiresAtUtc = expiresAtUtc;
                accessTokenStore.AccessToken = token;
            }

            if (expiresAtUtc is not null && expiresAtUtc <= DateTime.UtcNow)
            {
                await ClearPersistedTokenAsync(tokenStorage, accessTokenStore, cancellationToken);
                throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                authHeaderAttached = true;
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

        _logger.LogInformation(
            "AuthHeaderDelegatingHandler request {Method} {Path}; tokenSource={TokenSource}; hasToken={HasToken}; expiresAtUtc={ExpiresAtUtc}; authHeaderAttached={AuthHeaderAttached}",
            request.Method.Method,
            request.RequestUri?.PathAndQuery ?? request.RequestUri?.ToString() ?? "(unknown)",
            tokenSource,
            !string.IsNullOrWhiteSpace(token),
            expiresAtUtc?.ToString("O"),
            authHeaderAttached);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning(
                "AuthHeaderDelegatingHandler received 401 for {Method} {Path}; clearing token state.",
                request.Method.Method,
                request.RequestUri?.PathAndQuery ?? request.RequestUri?.ToString() ?? "(unknown)");

            await ClearPersistedTokenAsync(tokenStorage, accessTokenStore, cancellationToken);

            response.Dispose();
            throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);
        }

        return response;
    }

    private bool TryResolveCircuitTokenServices(out ITokenStorage tokenStorage, out IAccessTokenStore accessTokenStore)
    {
        tokenStorage = _fallbackTokenStorage;
        accessTokenStore = _fallbackAccessTokenStore;

        var services = _circuitServicesAccessor.Services;
        if (services is null)
            return false;

        var resolvedTokenStorage = services.GetService<ITokenStorage>();
        var resolvedAccessTokenStore = services.GetService<IAccessTokenStore>();
        if (resolvedTokenStorage is null || resolvedAccessTokenStore is null)
            return false;

        tokenStorage = resolvedTokenStorage;
        accessTokenStore = resolvedAccessTokenStore;
        return true;
    }

    private static async Task ClearPersistedTokenAsync(
        ITokenStorage tokenStorage,
        IAccessTokenStore accessTokenStore,
        CancellationToken cancellationToken)
    {
        accessTokenStore.AccessToken = null;
        accessTokenStore.ExpiresAtUtc = null;

        try
        {
            await tokenStorage.ClearAsync(cancellationToken);
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
