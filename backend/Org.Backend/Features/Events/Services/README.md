# Events Services

## IEventService / EventService
**Methods**:
- `Task<List<EventDto>> ListEventsAsync(Guid orgId, Guid userId)`
- `Task<EventDto> CreateEventAsync(Guid orgId, CreateEventRequest request, Guid userId)`
- `Task<EventDto> GetEventAsync(Guid eventId, Guid userId)`
- `Task<EventDto> UpdateEventAsync(Guid eventId, UpdateEventRequest request, Guid userId)`
- `Task DeleteEventAsync(Guid eventId, Guid userId)`
- `Task<List<EventDto>> ListPublicEventsAsync()`
- `Task<EventDto> GetPublicEventAsync(Guid eventId)`

## NOT Implemented in Phase 3C
- ❌ No real service implementations
