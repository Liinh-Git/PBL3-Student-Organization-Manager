using Org.Backend.Domain.Entities;
using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Mappings;

public static class UserMappings
{
    public static UserProfileDto ToUserProfileDto(this User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Status = user.Status.ToString(),
            ProfileVisibility = user.ProfileVisibility?.ToString(),
            LastLoginAtUtc = user.LastLoginAt
        };
    }

    public static MyOrganizationDto ToMyOrganizationDto(this Member member)
    {
        return new MyOrganizationDto
        {
            Id = member.Organization.Id,
            Name = member.Organization.OrgName,
            Description = member.Organization.Description,
            AvatarUrl = member.Organization.AvatarUrl,
            CoverUrl = member.Organization.CoverUrl,
            Location = member.Organization.Location,
            FoundingDate = member.Organization.FoundingDate,
            TotalMembers = member.Organization.TotalMembers,
            CreatedAtUtc = member.Organization.CreatedAt,
            RoleId = member.RoleId ?? Guid.Empty,
            RoleName = member.Role?.RoleName ?? "Member",
            MemberId = member.Id,
            JoinedAtUtc = member.JoinDate,
            IsDefault = null // Can be enhanced later with user preference
        };
    }

    public static MyEventDto ToMyEventDto(this Event evt)
    {
        return new MyEventDto
        {
            Id = evt.Id,
            OrganizationId = evt.OrgId,
            OrganizationName = evt.Organization.OrgName,
            Name = evt.EventName,
            Description = evt.Description,
            StartDate = evt.StartDate,
            EndDate = evt.EndDate,
            BannerUrl = evt.BannerUrl,
            Status = evt.Status.ToString(),
            Visibility = evt.Visibility.ToString(),
            Location = evt.Location,
            ParticipationRole = null,
            AttendanceStatus = null,
            EventRelation = null
        };
    }

    public static MyEventDto ToMyEventDto(this Event evt, string participationRole, string? attendanceStatus = null, string? eventRelation = null)
    {
        return evt.ToMyEventDto() with
        {
            ParticipationRole = participationRole,
            AttendanceStatus = attendanceStatus,
            EventRelation = eventRelation
        };
    }

    public static DiscoverOrganizationDto ToDiscoverOrganizationDto(this Organization org)
    {
        return new DiscoverOrganizationDto
        {
            Id = org.Id,
            Name = org.OrgName,
            Description = org.Description,
            AvatarUrl = org.AvatarUrl,
            CoverUrl = org.CoverUrl,
            Location = org.Location,
            TotalMembers = org.TotalMembers,
            Status = org.Status.ToString()
        };
    }
}
