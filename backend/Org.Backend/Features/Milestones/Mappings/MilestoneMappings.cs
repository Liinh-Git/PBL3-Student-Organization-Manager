using Org.Backend.Domain.Entities;
using Org.Shared.Features.Milestones;

namespace Org.Backend.Features.Milestones.Mappings;

public static class MilestoneMappings
{
    public static MilestoneDto ToMilestoneDto(this Milestone milestone)
    {
        return new MilestoneDto
        {
            Id = milestone.Id,
            EventId = milestone.EventId,
            Title = milestone.Title,
            Description = milestone.Description,
            StartDate = milestone.StartDate,
            EndDate = milestone.EndDate,
            Status = milestone.Status.ToString(),
            OrderIndex = milestone.OrderIndex,
            CreatedAtUtc = milestone.CreatedAt,
            UpdatedAtUtc = milestone.UpdatedAt
        };
    }
}
