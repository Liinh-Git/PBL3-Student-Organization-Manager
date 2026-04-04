using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Org.Shared.Features.Events;

namespace Org.Frontend.Services.Events
{
    public interface IEventService
    {
        Task<List<EventDto>> GetEventsAsync(Guid orgId);
        // Tạm thời khai báo để mốt nối API Tạo Chiến dịch/Sự kiện mới [cite: 28]
        // Sửa dòng CreateEventAsync thành:
        Task<EventDto> CreateEventAsync(CreateEventRequest request);

        Task<EventDto?> GetEventDetailAsync(Guid eventId);
    }
}