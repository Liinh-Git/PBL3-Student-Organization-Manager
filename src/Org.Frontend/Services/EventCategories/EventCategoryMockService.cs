using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Org.Shared.Features.EventCategories;

namespace Org.Frontend.Services.EventCategories
{
    public class EventCategoryMockService : IEventCategoryService
    {
        public Task<List<EventCategoryDto>> GetCategoriesAsync(Guid milestoneId)
        {
            return Task.FromResult(new List<EventCategoryDto>
            {
                // CẬP NHẬT MOCK DATA CÓ LEAD
                new EventCategoryDto { Id = Guid.NewGuid(), MilestoneId = milestoneId, Name = "Logistics & Venue", LeadName = "David Chen", LeadAvatarUrl = "https://randomuser.me/api/portraits/men/32.jpg", ActiveSubtasks = 4, ProgressPercentage = 60 },
                new EventCategoryDto { Id = Guid.NewGuid(), MilestoneId = milestoneId, Name = "PR & Marketing", LeadName = "Sarah Jenkins", LeadAvatarUrl = "https://randomuser.me/api/portraits/women/44.jpg", ActiveSubtasks = 2, ProgressPercentage = 85 },
                new EventCategoryDto { Id = Guid.NewGuid(), MilestoneId = milestoneId, Name = "Technical Team", LeadName = "Marcus V.", LeadAvatarUrl = "https://randomuser.me/api/portraits/men/75.jpg", ActiveSubtasks = 12, ProgressPercentage = 40 }
            });
        }
    }
}