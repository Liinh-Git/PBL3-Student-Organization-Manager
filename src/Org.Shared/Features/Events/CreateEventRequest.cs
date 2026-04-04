using System;

namespace Org.Shared.Features.Events
{
    public class CreateEventRequest
    {
        public string? Title { get; set; }
        public DateTime? Date { get; set; } = DateTime.Today;
        public TimeSpan? Time { get; set; } = new TimeSpan(8, 0, 0); // Mặc định 8h sáng
        public string? Location { get; set; }
        public int TotalSlots { get; set; }
    }
}