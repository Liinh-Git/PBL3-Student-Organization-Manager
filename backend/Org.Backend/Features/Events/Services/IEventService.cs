using Org.Shared.Features.Events;

namespace Org.Backend.Features.Events.Services;

public interface IEventService
{
    Task<List<EventSummaryDto>> GetOrganizationEventsAsync(Guid orgId, Guid userId, CancellationToken ct = default);
    Task<EventDto> GetEventByIdAsync(Guid eventId, Guid userId, CancellationToken ct = default);
    Task<List<EventPublicDto>> GetPublicEventsAsync(CancellationToken ct = default);
    Task<EventPublicDto> GetPublicEventByIdAsync(Guid eventId, CancellationToken ct = default);
    
    // Write operations
    Task<EventDto> CreateEventAsync(Guid orgId, Guid userId, CreateEventRequest request, CancellationToken ct = default);
    Task<EventDto> UpdateEventAsync(Guid eventId, Guid userId, UpdateEventRequest request, CancellationToken ct = default);
    Task<EventDto> UpdateEventStatusAsync(Guid eventId, Guid userId, UpdateEventStatusRequest request, CancellationToken ct = default);
    Task<bool> DeleteEventAsync(Guid eventId, Guid userId, CancellationToken ct = default);
}
