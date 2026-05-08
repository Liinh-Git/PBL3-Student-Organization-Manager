using Org.Shared.Features.Discover;

namespace Org.Backend.Features.Discover.Services;

public interface IDiscoverService
{
    Task<List<DiscoverEventDto>> DiscoverEventsAsync(Guid userId, CancellationToken ct = default);
}
