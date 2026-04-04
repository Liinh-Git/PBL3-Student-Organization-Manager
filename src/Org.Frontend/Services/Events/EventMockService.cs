using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Org.Shared.Features.Events;

namespace Org.Frontend.Services.Events
{
    public class EventMockService : IEventService
    {
        // Khai báo list tĩnh để lưu tạm data trong lúc chạy app
        private List<EventDto> _mockData = new()
        {
            new EventDto { Id = Guid.NewGuid(), Title = "Ngày Hội Xanh", Status = "ONGOING", Date = DateTime.Today.AddHours(9), Location = "Main Campus Plaza", RegisteredCount = 420, TotalSlots = 500, ImageUrl = "https://images.unsplash.com/photo-1540575467063-178a50c2df87?auto=format&fit=crop&w=500&q=80" },
            new EventDto { Id = Guid.NewGuid(), Title = "Hội Thảo Khởi Nghiệp", Status = "UPCOMING", Date = new DateTime(2023, 10, 24, 14, 0, 0), Location = "Tech Innovation Lab", RegisteredCount = 50, TotalSlots = 200, ImageUrl = "https://images.unsplash.com/photo-1552664730-d307ca884978?auto=format&fit=crop&w=500&q=80" }
        };

        public Task<List<EventDto>> GetEventsAsync(Guid orgId)
        {
            return Task.FromResult(_mockData);
        }

        public Task<EventDto> CreateEventAsync(CreateEventRequest req)
        {
            var newEvent = new EventDto
            {
                Id = Guid.NewGuid(),
                Title = req.Title ?? "Untitled Event",
                Status = "UPCOMING",
                Date = (req.Date ?? DateTime.Today).Add(req.Time ?? TimeSpan.Zero),
                Location = req.Location ?? "To be announced",
                RegisteredCount = 0,
                TotalSlots = req.TotalSlots,
                ImageUrl = "https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=500&q=80" // Ảnh mặc định
            };

            // Thêm event mới lên đầu danh sách
            _mockData.Insert(0, newEvent);
            
            return Task.FromResult(newEvent);
        }

       public Task<EventDto?> GetEventDetailAsync(Guid eventId)
        {
            // Tìm event trong danh sách có sẵn (bao gồm cả event bồ mới tạo từ popup)
            var evt = _mockData.FirstOrDefault(e => e.Id == eventId);
            
            if (evt != null)
            {
                // Fake thêm mấy thông số của Dashboard nếu event này mới tạo chưa có
                evt.Description ??= "Mô tả chi tiết sự kiện đang được cập nhật...";
                evt.CompletionPercentage = evt.CompletionPercentage == 0 ? new Random().Next(10, 90) : evt.CompletionPercentage;
                evt.BudgetUsedPercentage = evt.BudgetUsedPercentage == 0 ? new Random().Next(20, 80) : evt.BudgetUsedPercentage;
                evt.RiskLevel ??= "Low";
                evt.TotalFiles = evt.TotalFiles == 0 ? new Random().Next(5, 50) : evt.TotalFiles;
                evt.ActualSpending = evt.ActualSpending == 0 ? new Random().Next(500, 5000) : evt.ActualSpending;
                evt.EndDate = evt.Date.AddDays(7); // Mặc định event kéo dài 7 ngày
            }

            return Task.FromResult(evt);
        }
    }
}