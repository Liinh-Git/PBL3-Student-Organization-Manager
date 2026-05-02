// ---- Interface service events — trả về ViewModel để UI không phụ thuộc backend contract ----
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Events;

public interface IEventService
{
    Task<List<EventViewModel>> GetEventsAsync(Guid orgId);
    Task<MyEventsViewModel> GetMyEventsAsync();
    Task<EventViewModel?> GetPublicEventDetailAsync(Guid eventId);
    Task<bool> CanCreateEventAsync(Guid orgId);
    Task<bool> CanManageEventAsync(Guid eventId);
    Task<EventViewModel> CreateEventAsync(CreateEventViewModel request);
    Task<EventViewModel?> GetEventDetailAsync(Guid eventId);
    Task<EventViewModel> UpdateEventAsync(Guid eventId, UpdateEventViewModel req);
    Task DeleteEventAsync(Guid eventId);
    
    // ---- Attendance / Registration ----
    Task RegisterEventAsync(Guid eventId);
    Task UnregisterEventAsync(Guid eventId);
}
