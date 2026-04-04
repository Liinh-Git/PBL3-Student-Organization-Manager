using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Org.Shared.Features.EventCategories;

namespace Org.Frontend.Services.EventCategories
{
    public interface IEventCategoryService
    {
        Task<List<EventCategoryDto>> GetCategoriesAsync(Guid milestoneId);
    }
}