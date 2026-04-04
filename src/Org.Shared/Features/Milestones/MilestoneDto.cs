using System;

namespace Org.Shared.Features.Milestones
{
    public class MilestoneDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string? Name { get; set; } // VD: "Giai đoạn 1: Lên kế hoạch"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}