using Org.Backend.Domain.Entities;
using Org.Backend.Features.Tasks.Mappings;
using Org.Shared.Features.EventCategories;

namespace Org.Backend.Features.EventCategories.Mappings;

public static class EventCategoryMappings
{
    public static EventCategoryDto ToEventCategoryDto(this EventCategory category, bool includeTasks = false)
    {
        return new EventCategoryDto
        {
            Id = category.Id,
            MilestoneId = category.MilestoneId,
            CategoryName = category.CategoryName,
            Description = category.Description,
            OwnerDepartmentId = category.OwnerDepartmentId,
            OwnerDepartmentName = category.OwnerDepartment?.DeptName,
            OrderIndex = category.OrderIndex,
            CreatedAtUtc = category.CreatedAt,
            UpdatedAtUtc = category.UpdatedAt,
            Tasks = includeTasks ? category.Tasks?.Where(t => !t.IsDeleted).Select(t => t.ToTaskDto()).ToList() : null
        };
    }
}
