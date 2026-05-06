using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Org.Frontend.Services.Auth;

public sealed class AuthenticatedBackendClient : IAuthenticatedBackendClient
{
    private readonly HttpClient _httpClient;
    private readonly IAccessTokenStore _accessTokenStore;
    private readonly FrontendAuthStateProvider _authStateProvider;
    private readonly NavigationManager _navigation;
    private readonly ILogger<AuthenticatedBackendClient> _logger;

    public AuthenticatedBackendClient(
        IHttpClientFactory httpClientFactory,
        IAccessTokenStore accessTokenStore,
        FrontendAuthStateProvider authStateProvider,
        NavigationManager navigation,
        ILogger<AuthenticatedBackendClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
        _accessTokenStore = accessTokenStore;
        _authStateProvider = authStateProvider;
        _navigation = navigation;
        _logger = logger;
    }

    public async Task<T?> GetFromJsonAsync<T>(string uri, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await SendAsync(request, ct);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
    }

    public async Task<TResponse?> PostAsJsonAsync<TRequest, TResponse>(
        string uri,
        TRequest body,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(body)
        };

        using var response = await SendAsync(request, ct);
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
    }

    public async Task<TResponse?> PutAsJsonAsync<TRequest, TResponse>(
        string uri,
        TRequest body,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, uri)
        {
            Content = JsonContent.Create(body)
        };

        using var response = await SendAsync(request, ct);
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
    }

    public async Task DeleteAsync(string uri, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, uri);
        using var _ = await SendAsync(request, ct);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        var token = _accessTokenStore.AccessToken;
        var expiresAtUtc = _accessTokenStore.ExpiresAtUtc;
        var tokenExpired = expiresAtUtc is DateTime exp && exp <= DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(token) || tokenExpired)
        {
            _logger.LogWarning(
                "AuthenticatedBackendClient blocked request {Method} {Path}; hasToken={HasToken}; tokenExpired={TokenExpired}; attached=false",
                request.Method.Method,
                request.RequestUri?.PathAndQuery ?? request.RequestUri?.ToString() ?? "(unknown)",
                !string.IsNullOrWhiteSpace(token),
                tokenExpired);

            throw new AuthApiException("AUTH_NOT_READY", 0);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        _logger.LogInformation(
            "AuthenticatedBackendClient sending {Method} {Path}; hasToken=true; tokenExpired={TokenExpired}; attached=true",
            request.Method.Method,
            request.RequestUri?.PathAndQuery ?? request.RequestUri?.ToString() ?? "(unknown)",
            tokenExpired);

        var response = await _httpClient.SendAsync(request, ct);

        _logger.LogInformation(
            "AuthenticatedBackendClient response {Method} {Path}; status={StatusCode}",
            request.Method.Method,
            request.RequestUri?.PathAndQuery ?? request.RequestUri?.ToString() ?? "(unknown)",
            (int)response.StatusCode);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            await HandleUnauthorizedAsync(ct);
            throw new AuthApiException("Phiên dang nh?p dã h?t h?n. Vui lòng dang nh?p l?i.", 401);
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            response.Dispose();
            throw new AuthApiException("B?n không có quy?n th?c hi?n thao tác này.", 403);
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = await ReadErrorMessageAsync(response, ct);
            var statusCode = (int)response.StatusCode;
            response.Dispose();
            throw new AuthApiException(message, statusCode);
        }

        return response;
    }

    private async Task HandleUnauthorizedAsync(CancellationToken ct)
    {
        try
        {
            await _authStateProvider.SignOutAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sign out after backend 401.");
        }

        try
        {
            _navigation.NavigateTo("/login", forceLoad: true);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Navigation to /login after 401 failed in current context.");
        }
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var fallback = $"Backend API failed with status code {(int)response.StatusCode}.";
        var raw = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(raw))
            return fallback;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("message", out var message)
                    && message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString() ?? fallback;
                }

                if (doc.RootElement.TryGetProperty("reason", out var reason)
                    && reason.ValueKind == JsonValueKind.String)
                {
                    return reason.GetString() ?? fallback;
                }
            }
        }
        catch (JsonException)
        {
            // Keep fallback to raw text.
        }

        return raw;
    }
}
