using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Org.Frontend.Services.Mocks;

namespace Org.Frontend.Services.Organizations;

public sealed class MockOrganizationContext(
    FrontendMockDataStore mockDataStore,
    NavigationManager navigationManager) : IOrganizationContext
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private Guid? _cachedOrganizationId;

    public async Task<Guid> GetOrganizationIdAsync(CancellationToken ct = default)
    {
        if (_cachedOrganizationId.HasValue)
        {
            return _cachedOrganizationId.Value;
        }

        await _lock.WaitAsync(ct);
        try
        {
            if (_cachedOrganizationId.HasValue)
            {
                return _cachedOrganizationId.Value;
            }

            // Attempt to parse orgId from URI
            if (Uri.TryCreate(_navigationManager.Uri, UriKind.Absolute, out var uri))
            {
                var query = QueryHelpers.ParseQuery(uri.Query);
                if (query.TryGetValue("orgId", out var rawId) && Guid.TryParse(rawId, out var parsedId))
                {
                    _cachedOrganizationId = parsedId;
                    return parsedId;
                }
            }

            // Fallback: Get first org if not specified
            var organizationId = await _mockDataStore.UseAsync(data => data.Organizations
                .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.Id)
                .FirstOrDefault(), ct);

            if (organizationId == Guid.Empty)
            {
                throw new InvalidOperationException("No organizations available in mock dataset.");
            }

            _cachedOrganizationId = organizationId;
            return organizationId;
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
}
