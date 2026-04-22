// ---- Interface service events — trả về ViewModel để UI không phụ thuộc backend contract ----
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Events;

public interface IEventService
{
    Task<List<EventViewModel>> GetEventsAsync(Guid orgId);
    Task<EventViewModel> CreateEventAsync(CreateEventViewModel request);
    Task<EventViewModel?> GetEventDetailAsync(Guid eventId);
    Task<EventViewModel> UpdateEventAsync(Guid eventId, UpdateEventViewModel req);
    Task DeleteEventAsync(Guid eventId);
}