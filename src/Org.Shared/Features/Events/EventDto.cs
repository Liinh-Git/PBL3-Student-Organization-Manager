using System;

namespace Org.Shared.Features.Events
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public int RegisteredCount { get; set; }
        public int TotalSlots { get; set; }
        public string? ImageUrl { get; set; }

        // ---- THÊM CÁC TRƯỜNG MỚI CHO DASHBOARD VÀO ĐÂY ----
        public string? Description { get; set; } // Mô tả ngắn dưới tiêu đề
        public DateTime EndDate { get; set; } // Ngày kết thúc để tính Days Left
        public int CompletionPercentage { get; set; } // 75%
        public int BudgetUsedPercentage { get; set; } // 82%
        public string? RiskLevel { get; set; } // "Low", "Medium", "High"
        public int TotalFiles { get; set; } // 124
        public decimal ActualSpending { get; set; } // 1,250
    }
}