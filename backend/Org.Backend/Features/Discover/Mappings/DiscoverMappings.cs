using Org.Backend.Domain.Entities;
using Org.Shared.Features.Discover;

namespace Org.Backend.Features.Discover.Mappings;

public static class DiscoverMappings
{
    public static DiscoverEventDto ToDiscoverEventDto(this Event evt)
    {
        return new DiscoverEventDto
        {
            Id = evt.Id,
            OrganizationId = evt.OrgId,
            OrganizationName = evt.Organization?.OrgName ?? string.Empty,
            Name = evt.EventName,
            Description = evt.Description,
            StartDate = evt.StartDate,
            EndDate = evt.EndDate,
            Location = evt.Location,
            Visibility = evt.Visibility.ToString(),
            Status = evt.Status.ToString()
        };
    }
}
