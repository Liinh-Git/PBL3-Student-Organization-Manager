using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Shared.Features.Events;

namespace Org.Backend.Features.Events.Mappings;

public static class EventMappings
{
    public static EventDto ToEventDto(this Event evt)
    {
        return new EventDto
        {
            Id = evt.Id,
            OrganizationId = evt.OrgId,
            Name = evt.EventName,
            Description = evt.Description,
            StartDate = evt.StartDate,
            EndDate = evt.EndDate,
            Status = evt.Status.ToString(),
            Visibility = evt.Visibility.ToString(),
            Location = evt.Location,
            BannerUrl = evt.BannerUrl,
            TargetParticipants = evt.TargetParticipants,
            RegisteredParticipants = evt.RegisteredParticipants,
            Budget = evt.Budget,
            AverageRating = evt.AverageRating,
            Tags = evt.Tags,
            CreatedAtUtc = evt.CreatedAt,
            UpdatedAtUtc = evt.UpdatedAt ?? evt.CreatedAt
        };
    }

    public static EventSummaryDto ToEventSummaryDto(this Event evt)
    {
        return new EventSummaryDto
        {
            Id = evt.Id,
            OrganizationId = evt.OrgId,
            Name = evt.EventName,
            Description = evt.Description,
            StartDate = evt.StartDate,
            EndDate = evt.EndDate,
            Status = evt.Status.ToString(),
            Visibility = evt.Visibility.ToString(),
            Location = evt.Location,
            BannerUrl = evt.BannerUrl,
            TargetParticipants = evt.TargetParticipants
        };
    }

    public static EventPublicDto ToEventPublicDto(this Event evt)
    {
        return new EventPublicDto
        {
            Id = evt.Id,
            OrganizationId = evt.OrgId,
            OrganizationName = evt.Organization.OrgName,
            Name = evt.EventName,
            Description = evt.Description,
            StartDate = evt.StartDate,
            EndDate = evt.EndDate,
            Location = evt.Location,
            BannerUrl = evt.BannerUrl,
            TargetParticipants = evt.TargetParticipants,
            RegisteredParticipants = evt.RegisteredParticipants,
            Visibility = evt.Visibility.ToString(),
            Status = evt.Status.ToString()
        };
    }
}
