// ---- Mock service cho dashboard user: đọc dữ liệu từ mock dataset, dùng Attendee semantics ----
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Dashboard;

public sealed class UserDashboardMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IUserDashboardService
{
    private const string DefaultOrganizationImageUrl = "/images/mockimages/Org1/Avt.jpg";
    private const string DefaultEventImageUrl = "/images/mockimages/Org1/Card1.jpg";

    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<UserDashboardViewModel> GetDashboardAsync(CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.TryParse(userIdText, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        return await _mockDataStore.UseAsync(data =>
        {
            var organizations = userId is null
                ? []
                : data.Members
                    .Where(x => x.UserId == userId.Value)
                    .OrderByDescending(x => x.JoinDate)
                    .Select(x =>
                    {
                        var org = data.Organizations.FirstOrDefault(o => o.Id == x.OrgId);
                        if (org is null)
                        {
                            return null;
                        }

                        return new UserOrganizationViewModel
                        {
                            OrganizationId = org.Id,
                            Name = org.OrgName,
                            Description = org.Description,
                            AvatarUrl = ResolveOrganizationImage(org),
                            JoinedAtUtc = new DateTimeOffset(DateTime.SpecifyKind(x.JoinDate, DateTimeKind.Utc)),
                            Role = ResolveDashboardRoleName(x.RoleId, data)
                        };

                    })
                    .Where(x => x is not null)
                    .Select(x => x!)
                    .ToList();

            // 2. REGISTERED EVENTS (User is Attendee OR Staff)
            var registeredEvents = new List<UserRegisteredEventViewModel>();
            if (userId != null)
            {
                // -- Part A: Events user is an Attendee of (Guest/Participant) --
                var attendeeEventIds = data.Attendees
                    .Where(x => x.UserId == userId.Value && !string.Equals(x.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.EventId)
                    .ToHashSet();

                // -- Part B: Events user is an EventMember of (Staff/Organizer) --
                // First find all MemberIds for this User across all Orgs
                var userMemberIds = data.Members
                    .Where(x => x.UserId == userId.Value)
                    .Select(x => x.Id)
                    .ToHashSet();
                
                var staffEventIds = data.EventMembers
                    .Where(x => userMemberIds.Contains(x.MemberId))
                    .Select(x => x.EventId)
                    .ToHashSet();

                var allMyEventIds = attendeeEventIds.Union(staffEventIds).ToList();

                foreach (var eventId in allMyEventIds)
                {
                    var eventItem = data.Events.FirstOrDefault(e => e.Id == eventId);
                    if (eventItem == null) continue;

                    var organization = data.Organizations.FirstOrDefault(o => o.Id == eventItem.OrgId);
                    
                    // Determine internal vs external role
                    string regStatus = "REGISTERED";
                    var attendeeRecord = data.Attendees.FirstOrDefault(a => a.EventId == eventId && a.UserId == userId.Value);
                    if (attendeeRecord != null) regStatus = NormalizeRegistrationStatus(attendeeRecord.Status);
                    
                    var eventMemberRecord = data.EventMembers.FirstOrDefault(em => em.EventId == eventId && userMemberIds.Contains(em.MemberId));
                    if (eventMemberRecord != null)
                    {
                        // If user is staff, show their staff role instead of just "Registered"
                        regStatus = eventMemberRecord.EventRole ?? "STAFF";
                    }

                    registeredEvents.Add(new UserRegisteredEventViewModel
                    {
                        EventId = eventItem.Id,
                        OrganizationId = eventItem.OrgId,
                        OrganizationName = organization?.OrgName ?? "Organization",
                        Name = eventItem.Name,
                        Description = eventItem.Description,
                        StartDate = eventItem.StartDate,
                        EndDate = eventItem.EndDate,
                        EventStatus = NormalizeEventStatus(eventItem.StatusLabel),
                        RegistrationStatus = regStatus,
                        RegisteredAtUtc = attendeeRecord != null 
                            ? new DateTimeOffset(DateTime.SpecifyKind(attendeeRecord.CreatedAt, DateTimeKind.Utc))
                            : new DateTimeOffset(eventItem.StartDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
                        Location = eventItem.Location,
                        ImageUrl = ResolveEventImage(eventItem, organization)
                    });
                }
            }
            
            registeredEvents = registeredEvents
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();


            var joinedOrganizationIds = organizations
                .Select(x => x.OrganizationId)
                .ToHashSet();

            var suggestedOrganizations = data.Organizations
                .Where(x => !joinedOrganizationIds.Contains(x.Id))
                .Select(x => new SuggestedOrganizationViewModel
                {
                    OrganizationId = x.Id,
                    Name = x.OrgName,
                    Description = x.Description,
                    ImageUrl = ResolveOrganizationImage(x),
                    MemberCount = x.TotalMembers > 0
                        ? x.TotalMembers
                        : data.Members.Count(m => m.OrgId == x.Id),
                    Location = x.Location,
                    IsActive = x.Status == 0
                })
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.MemberCount)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .ToList();

            var registeredEventIds = registeredEvents
                .Select(x => x.EventId)
                .ToHashSet();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var suggestedEvents = data.Events
                .Where(x => !registeredEventIds.Contains(x.Id) && x.EndDate >= today)
                .Select(x =>
                {
                    var organization = data.Organizations.FirstOrDefault(o => o.Id == x.OrgId);
                    return new SuggestedEventViewModel
                    {
                        EventId = x.Id,
                        OrganizationId = x.OrgId,
                        OrganizationName = organization?.OrgName ?? "Organization",
                        Name = x.Name,
                        Description = x.Description,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        EventStatus = NormalizeEventStatus(x.StatusLabel),
                        Location = x.Location,
                        ImageUrl = ResolveEventImage(x, organization),
                        RegisteredCount = data.Attendees.Count(a =>
                            a.EventId == x.Id
                            && !string.Equals(a.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                    };
                })
                .OrderBy(x => x.StartDate < today ? 1 : 0)
                .ThenBy(x => x.StartDate)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .ToList();

            return new UserDashboardViewModel
            {
                DisplayName = user.Identity?.Name?.Trim() ?? "Nguoi dung",
                Email = user.FindFirst(ClaimTypes.Email)?.Value,
                Organizations = organizations,
                RegisteredEvents = registeredEvents,
                SuggestedOrganizations = suggestedOrganizations,
                SuggestedEvents = suggestedEvents
            };
        }, ct);
    }

    private static string ResolveOrganizationImage(MockOrganization organization)
    {
        if (!string.IsNullOrWhiteSpace(organization.AvatarUrl))
            return organization.AvatarUrl;

        if (!string.IsNullOrWhiteSpace(organization.CoverUrl))
            return organization.CoverUrl;

        return DefaultOrganizationImageUrl;
    }

    private static string ResolveEventImage(MockEvent eventItem, MockOrganization? organization)
    {
        if (!string.IsNullOrWhiteSpace(eventItem.ImageUrl))
            return eventItem.ImageUrl;

        if (!string.IsNullOrWhiteSpace(organization?.CoverUrl))
            return organization.CoverUrl;

        if (!string.IsNullOrWhiteSpace(organization?.AvatarUrl))
            return organization.AvatarUrl;

        return DefaultEventImageUrl;
    }

    private static string NormalizeEventStatus(string? statusLabel)
    {
        return statusLabel?.Trim().ToUpperInvariant() switch
        {
            "ONGOING" => "ONGOING",
            "COMPLETED" => "COMPLETED",
            "PLANNING" => "PLANNING",
            _ => "DRAFT"
        };
    }

    private static string NormalizeRegistrationStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "ATTENDED" => "ATTENDED",
            "CANCELLED" => "CANCELLED",
            _ => "REGISTERED"
        };
    }

    private static string ResolveDashboardRoleName(Guid? roleId, MockDataSet data)
    {
        if (!roleId.HasValue)
            return "Member";

        var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == roleId.Value);
        if (role is null || string.IsNullOrWhiteSpace(role.RoleName))
            return "Member";

        return role.RoleName.Trim() switch
        {
            "President" => "Owner",
            "VicePresident" => "Admin",
            "Manager" => "Admin",
            _ => "Member"
        };
    }
}
