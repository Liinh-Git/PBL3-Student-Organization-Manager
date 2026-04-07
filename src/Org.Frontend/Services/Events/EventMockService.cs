// ---- EventMockService: mock data UI cho EventList/EventDetail ----
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Events;

public class EventMockService : IEventService
{
    // Dùng list tĩnh để data không bị reset khi chuyển trang
    private List<EventViewModel> _mockData = new()
    {
        new EventViewModel
        {
            Id = Guid.NewGuid(), Name = "Ngày Hội Xanh", StatusLabel = "ONGOING",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            Location = "Main Campus Plaza", RegisteredCount = 420, TotalSlots = 500,
            ImageUrl = "https://images.unsplash.com/photo-1540575467063-178a50c2df87?auto=format&fit=crop&w=500&q=80"
        },
        new EventViewModel
        {
            Id = Guid.NewGuid(), Name = "Hội Thảo Khởi Nghiệp", StatusLabel = "UPCOMING",
            StartDate = new DateOnly(2026, 10, 24),
            EndDate = new DateOnly(2026, 10, 25),
            Location = "Tech Innovation Lab", RegisteredCount = 50, TotalSlots = 200,
            ImageUrl = "https://images.unsplash.com/photo-1552664730-d307ca884978?auto=format&fit=crop&w=500&q=80"
        }
    };

    public Task<List<EventViewModel>> GetEventsAsync(Guid orgId)
        => Task.FromResult(_mockData);

    public Task<EventViewModel> CreateEventAsync(CreateEventViewModel req)
    {
        var newEvent = new EventViewModel
        {
            Id = Guid.NewGuid(),
            Name = req.Title.Trim().Length > 0 ? req.Title.Trim() : "Untitled Event",
            StatusLabel = "UPCOMING",
            StartDate = DateOnly.FromDateTime(req.Date ?? DateTime.Today),
            EndDate = DateOnly.FromDateTime((req.Date ?? DateTime.Today).AddDays(1)),
            Location = req.Location ?? "To be announced",
            RegisteredCount = 0,
            TotalSlots = req.TotalSlots,
            ImageUrl = "https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=500&q=80"
        };
        _mockData.Insert(0, newEvent);
        return Task.FromResult(newEvent);
    }

    public Task<EventViewModel?> GetEventDetailAsync(Guid eventId)
    {
        var evt = _mockData.FirstOrDefault(e => e.Id == eventId);
        if (evt != null)
        {
            // Fake thêm dashboard stats nếu chưa có
            evt.Description ??= "Mô tả chi tiết sự kiện đang được cập nhật...";
            evt.CompletionLabel ??= $"{new Random().Next(10, 90)}%";
            evt.BudgetLabel ??= $"{new Random().Next(20, 80)}%";
            evt.RiskLevel ??= "Low";
            if (evt.TotalFiles == 0) evt.TotalFiles = new Random().Next(5, 50);
            if (evt.ActualSpending == 0) evt.ActualSpending = new Random().Next(500, 5000);
        }
        return Task.FromResult(evt);
    }
}