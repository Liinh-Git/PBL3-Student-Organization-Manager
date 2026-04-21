// ---- Interface service event categories ----
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Events;

public interface IEventCategoryService
{
    Task<List<EventCategoryViewModel>> GetCategoriesAsync(Guid milestoneId);
    Task<EventCategoryViewModel> GetCategoryDetailAsync(Guid categoryId);
    Task<EventCategoryViewModel> CreateCategoryAsync(CreateEventCategoryViewModel req);
    Task<EventCategoryViewModel> UpdateCategoryAsync(Guid categoryId, UpdateEventCategoryViewModel req);
    Task DeleteCategoryAsync(Guid categoryId);
}
