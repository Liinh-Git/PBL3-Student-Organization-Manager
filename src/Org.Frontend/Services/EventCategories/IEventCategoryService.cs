// ---- Interface service event categories ----
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.EventCategories;

public interface IEventCategoryService
{
    Task<List<EventCategoryViewModel>> GetCategoriesAsync(Guid milestoneId);
}