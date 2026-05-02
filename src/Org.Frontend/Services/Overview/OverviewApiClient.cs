namespace Org.Frontend.Services.Overview;

public sealed class OverviewApiClient : IOverviewService
{
    public Task<OverviewPageViewModel> GetOverviewAsync(CancellationToken ct = default)
        => throw new NotSupportedException("Overview API chua san sang o backend. Vui long bat mock mode hoac bo sung endpoint overview.");
}
