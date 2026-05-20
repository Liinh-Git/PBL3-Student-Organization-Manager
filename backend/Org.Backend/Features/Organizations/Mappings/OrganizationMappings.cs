using Org.Backend.Domain.Entities;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations.Mappings;

public static class OrganizationMappings
{
    public static OrganizationDto ToOrganizationDto(this Organization org)
    {
        return new OrganizationDto
        {
            Id = org.Id,
            Name = org.OrgName,
            Description = org.Description,
            AvatarUrl = org.AvatarUrl,
            CoverUrl = org.CoverUrl,
            FoundingDate = org.FoundingDate,
            Location = org.Location,
            ContactEmail = org.ContactEmail,
            ContactPhone = org.ContactPhone,
            TotalMembers = org.TotalMembers,
            Status = org.Status.ToString(),
            CreatedAtUtc = org.CreatedAt,
            UpdatedAtUtc = org.UpdatedAt ?? org.CreatedAt
        };
    }

    public static OrganizationSummaryDto ToOrganizationSummaryDto(this Organization org)
    {
        return new OrganizationSummaryDto
        {
            Id = org.Id,
            Name = org.OrgName,
            Description = org.Description,
            AvatarUrl = org.AvatarUrl,
            TotalMembers = org.TotalMembers,
            Status = org.Status.ToString()
        };
    }

    public static OrganizationPublicOverviewDto ToOrganizationPublicOverviewDto(this Organization org, int publicEventsCount, int departmentsCount)
    {
        return new OrganizationPublicOverviewDto
        {
            Id = org.Id,
            Name = org.OrgName,
            Description = org.Description,
            AvatarUrl = org.AvatarUrl,
            CoverUrl = org.CoverUrl,
            TotalMembers = org.TotalMembers,
            PublicEventsCount = publicEventsCount,
            DepartmentsCount = departmentsCount
        };
    }
}
