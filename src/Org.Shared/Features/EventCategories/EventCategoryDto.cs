using System;

namespace Org.Shared.Features.EventCategories
{
    public class EventCategoryDto
    {
        public Guid Id { get; set; }
        public Guid MilestoneId { get; set; }
        public string? Name { get; set; }

        // ---- THÊM CÁC TRƯỜNG MỚI VÀO ĐÂY ----
        public string? LeadName { get; set; } // VD: "David Chen"
        public string? LeadAvatarUrl { get; set; } // Ảnh đại diện trưởng ban
        public int ActiveSubtasks { get; set; } // 4 Active Subtasks
        public int ProgressPercentage { get; set; } // Phần trăm tiến độ ban
    }
}