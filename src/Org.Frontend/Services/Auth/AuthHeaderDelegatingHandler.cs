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

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
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
                throw new AuthApiException("Phien dang nhap da het han. Vui long dang nhap lai.", 401);
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                authHeaderAttached = true;
            }
        }
        catch (InvalidOperationException)
        {
            // JS interop may be unavailable during prerender.
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected.
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
            if (authHeaderAttached)
            {
                _logger.LogWarning(
                    "AuthHeaderDelegatingHandler received 401 after attaching token for {Method} {Path}; clearing token state.",
                    request.Method.Method,
                    request.RequestUri?.PathAndQuery ?? request.RequestUri?.ToString() ?? "(unknown)");

                await ClearPersistedTokenAsync(tokenStorage, accessTokenStore, cancellationToken);

                response.Dispose();
                throw new AuthApiException("Phien dang nhap da het han. Vui long dang nhap lai.", 401);
            }

            _logger.LogWarning(
                "AuthHeaderDelegatingHandler received 401 without attached token for {Method} {Path}; keeping current token state.",
                request.Method.Method,
                request.RequestUri?.PathAndQuery ?? request.RequestUri?.ToString() ?? "(unknown)");

            return response;
        }

        return response;
    }

    private bool TryResolveCircuitTokenServices(
        out ITokenStorage tokenStorage,
        out IAccessTokenStore accessTokenStore)
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
            // JS interop may be unavailable during prerender.
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected.
        }
    }
}