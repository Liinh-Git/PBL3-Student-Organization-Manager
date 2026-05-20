using Org.Shared.Features.EventCategories;

namespace Org.Backend.Features.EventCategories.Services;

public interface IEventCategoryService
{
    Task<List<EventCategoryDto>> GetMilestoneCategoriesAsync(Guid milestoneId, Guid userId, CancellationToken ct = default);
    Task<EventCategoryDto> GetCategoryByIdAsync(Guid categoryId, Guid userId, CancellationToken ct = default);
    Task<EventCategoryDto> CreateCategoryAsync(Guid milestoneId, CreateEventCategoryRequest request, Guid userId, CancellationToken ct = default);
    Task<EventCategoryDto> UpdateCategoryAsync(Guid categoryId, UpdateEventCategoryRequest request, Guid userId, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid categoryId, Guid userId, CancellationToken ct = default);
}
