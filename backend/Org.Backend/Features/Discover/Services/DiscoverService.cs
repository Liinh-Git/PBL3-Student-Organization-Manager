using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Discover.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Discover;

namespace Org.Backend.Features.Discover.Services;

public class DiscoverService : IDiscoverService
{
    private readonly AppDbContext _context;

    public DiscoverService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DiscoverEventDto>> DiscoverEventsAsync(Guid userId, CancellationToken ct = default)
    {
        // Get public events that are published or ongoing
        var events = await _context.Events
            .Include(e => e.Organization)
            .Where(e => e.Visibility == EventVisibility.Public && 
                       (e.Status == EventStatus.Published || e.Status == EventStatus.Ongoing))
            .OrderBy(e => e.StartDate)
            .ToListAsync(ct);

        return events.Select(e => e.ToDiscoverEventDto()).ToList();
    }
}
