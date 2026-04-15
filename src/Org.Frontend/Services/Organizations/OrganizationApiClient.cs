using System.Net.Http.Json;
using Org.Shared.Features.Organizations;

namespace Org.Frontend.Services.Organizations;

public sealed class OrganizationApiClient(HttpClient httpClient) : IOrganizationContext
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private Guid? _cachedOrganizationId;

    public async Task<Guid> GetOrganizationIdAsync(CancellationToken ct = default)
    {
        if (_cachedOrganizationId.HasValue)
            return _cachedOrganizationId.Value;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cachedOrganizationId.HasValue)
                return _cachedOrganizationId.Value;

            var payload = await _httpClient.GetFromJsonAsync<GetDefaultOrganizationResponse>("api/organizations/default", ct)
                ?? throw new InvalidOperationException("Backend returned empty organization payload.");

            _cachedOrganizationId = payload.Data.Id;
            return _cachedOrganizationId.Value;
        }
        finally
        {
            _lock.Release();
        }
    }
}
