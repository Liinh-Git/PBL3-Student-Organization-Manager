using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Org.Frontend.Services.Auth;
using Org.Shared.Features.Organizations;

namespace Org.Frontend.Services.Organizations;

public sealed class OrganizationApiClient(
    HttpClient httpClient,
    ITokenStorage tokenStorage,
    IAccessTokenStore accessTokenStore) : IOrganizationContext
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ITokenStorage _tokenStorage = tokenStorage;
    private readonly IAccessTokenStore _accessTokenStore = accessTokenStore;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private Guid? _cachedOrganizationId;
    private const int UnauthorizedRetryDelayMilliseconds = 150;

    public async Task<Guid> GetOrganizationIdAsync(CancellationToken ct = default)
    {
        if (_cachedOrganizationId.HasValue)
            return _cachedOrganizationId.Value;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cachedOrganizationId.HasValue)
                return _cachedOrganizationId.Value;

            var payload = await GetDefaultOrganizationWithRetryAsync(ct);

            _cachedOrganizationId = payload.Data.Id;
            return _cachedOrganizationId.Value;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task ResetAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            _cachedOrganizationId = null;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<GetDefaultOrganizationResponse> GetDefaultOrganizationWithRetryAsync(CancellationToken ct)
    {
        const int maxAttempts = 2;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/organizations/default");
            var token = await GetAccessTokenAsync(ct);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.Unauthorized && attempt < maxAttempts)
            {
                await Task.Delay(UnauthorizedRetryDelayMilliseconds, ct);
                continue;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _cachedOrganizationId = null;
                throw new AuthApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GetDefaultOrganizationResponse>(cancellationToken: ct)
                ?? throw new InvalidOperationException("Backend returned empty organization payload.");
        }

        throw new InvalidOperationException("Could not load default organization.");
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(_accessTokenStore.AccessToken))
            return _accessTokenStore.AccessToken;

        try
        {
            var token = await _tokenStorage.GetTokenAsync(ct);
            if (!string.IsNullOrWhiteSpace(token))
                _accessTokenStore.AccessToken = token;

            return token;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}
