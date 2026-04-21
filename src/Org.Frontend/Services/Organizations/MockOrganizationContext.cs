// ---- Mock implementation cho IOrganizationContext — trả orgId từ mock data ----
// Dùng double-check lock (SemaphoreSlim) để chỉ resolve orgId một lần dù concurrency.
// Tự động lấy tổ chức đầu tiên (sắp xếp theo Code) trong mock dataset.
using Org.Frontend.Services.Mocks;

namespace Org.Frontend.Services.Organizations;

public sealed class MockOrganizationContext(FrontendMockDataStore mockDataStore) : IOrganizationContext
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
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
