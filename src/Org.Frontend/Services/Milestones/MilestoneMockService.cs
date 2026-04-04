using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Org.Shared.Features.Milestones;

namespace Org.Frontend.Services.Milestones
{
    public class MilestoneMockService : IMilestoneService
    {
        public Task<List<MilestoneDto>> GetMilestonesAsync(Guid eventId)
        {
            return Task.FromResult(new List<MilestoneDto>
            {
                new MilestoneDto { Id = Guid.NewGuid(), EventId = eventId, Name = "Giai đoạn 1: Lên kế hoạch & Chuẩn bị", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(7) },
                new MilestoneDto { Id = Guid.NewGuid(), EventId = eventId, Name = "Giai đoạn 2: Triển khai sự kiện", StartDate = DateTime.Today.AddDays(8), EndDate = DateTime.Today.AddDays(14) }
            });
        }
    }
}