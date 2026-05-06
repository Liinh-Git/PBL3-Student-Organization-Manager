using System.Net.Http.Json;
using Org.Frontend.Services.Auth;
using Org.Shared;
using Org.Shared.Features.Departments;
using Org.Shared.Features.Events;
using Org.Shared.Features.Members;
using Org.Shared.Features.Organizations;
using Org.Shared.Features.Users;

namespace Org.Frontend.Services.Organizations;

public sealed class OrganizationServiceApiClient(IAuthenticatedBackendClient backendClient) : IOrganizationService
{
    private readonly IAuthenticatedBackendClient _backendClient = backendClient;

    public async Task<OrganizationOverviewViewModel> GetOrganizationOverviewAsync(Guid organizationId, CancellationToken ct = default)
    {
        var permission = await GetOrganizationViewerPermissionAsync(organizationId, ct);
        var publicPayload = await _backendClient.GetFromJsonAsync<GetPublicOrganizationOverviewResponse>(
            $"api/organizations/{organizationId:D}/public-overview",
            ct) ?? throw new InvalidOperationException("Backend returned empty public organization payload.");
        var dto = publicPayload.Data;

        var eventCount = 0;
        var upcomingEventCount = 0;
        var activeTaskCount = 0;
        var milestoneCount = 0;
        var departments = new List<OrganizationDepartmentSummaryViewModel>();
        var leadership = new List<OrganizationAdminViewModel>();
        var highlights = new List<OrganizationEventHighlightViewModel>();

        if (permission.IsMember)
        {
            var eventsPayload = await _backendClient.GetFromJsonAsync<GetOrganizationEventsResponse>(
                $"api/organizations/{organizationId:D}/events",
                ct);
            var departmentsPayload = await _backendClient.GetFromJsonAsync<GetDepartmentsResponse>(
                $"api/organizations/{organizationId:D}/departments",
                ct);
            var membersPayload = await _backendClient.GetFromJsonAsync<GetMembersResponse>(
                $"api/organizations/{organizationId:D}/members",
                ct);

            if (eventsPayload is not null)
            {
                eventCount = eventsPayload.Items.Count;
                var today = DateOnly.FromDateTime(DateTime.Today);
                upcomingEventCount = eventsPayload.Items.Count(x => x.StartDate >= today);
                activeTaskCount = eventsPayload.Items.Sum(x => Math.Max(0, x.TaskCount - x.CompletedTaskCount));
                milestoneCount = eventsPayload.Items.Sum(x => x.MilestoneCount);

                highlights = eventsPayload.Items
                    .Where(x => x.StartDate >= today)
                    .OrderBy(x => x.StartDate)
                    .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .Take(6)
                    .Select(x => new OrganizationEventHighlightViewModel
                    {
                        EventId = x.Id,
                        Name = x.Name,
                        StartDate = x.StartDate,
                        Location = null,
                        ImageUrl = null
                    })
                    .ToList();
            }

            if (departmentsPayload is not null)
            {
                departments = departmentsPayload.Items
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new OrganizationDepartmentSummaryViewModel
                    {
                        DepartmentId = x.Id,
                        Name = x.Name,
                        MemberCount = x.MemberCount
                    })
                    .ToList();
            }

            if (membersPayload is not null)
            {
                leadership = membersPayload.Items
                    .Where(x => x.IsActive && IsLeadershipRole(x.Role))
                    .OrderBy(x => GetRoleRank(x.Role))
                    .ThenBy(x => x.FullName, StringComparer.OrdinalIgnoreCase)
                    .Take(6)
                    .Select(x => new OrganizationAdminViewModel
                    {
                        UserId = Guid.Empty,
                        Name = x.FullName,
                        Role = x.Role.ToString(),
                        Avatar = null
                    })
                    .ToList();
            }
        }

        return new OrganizationOverviewViewModel
        {
            OrganizationId = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            ShortDescription = dto.Description,
            AvatarUrl = dto.AvatarUrl,
            CoverUrl = dto.CoverUrl,
            Location = dto.Location,
            FoundedDate = dto.FoundingDate,
            MemberCount = dto.TotalMembers,
            EventCount = eventCount,
            UpcomingEventCount = upcomingEventCount,
            ActiveTaskCount = activeTaskCount,
            MilestoneCount = milestoneCount,
            LastActivityAtUtc = dto.UpdatedAtUtc?.UtcDateTime ?? dto.CreatedAtUtc.UtcDateTime,
            ViewerPermission = permission,
            Departments = departments,
            Timeline = [],
            HighlightEvents = highlights,
            Leadership = leadership
        };
    }

    public async Task<OrganizationOverviewViewModel> UpdateOrganizationOverviewAsync(
        Guid organizationId,
        UpdateOrganizationOverviewRequest request,
        CancellationToken ct = default)
    {
        var permission = await GetOrganizationViewerPermissionAsync(organizationId, ct);
        if (!permission.CanEditOverview)
            throw new UnauthorizedAccessException("You do not have permission to edit this organization overview.");

        var current = await _backendClient.GetFromJsonAsync<GetOrganizationByIdResponse>(
            $"api/organizations/{organizationId:D}",
            ct) ?? throw new InvalidOperationException("Backend returned empty organization payload.");

        var payload = new UpdateOrganizationRequest(
            string.IsNullOrWhiteSpace(request.Name) ? current.Data.Name : request.Name.Trim(),
            request.Description,
            request.AvatarUrl,
            request.CoverUrl,
            current.Data.FoundingDate,
            request.Location,
            current.Data.IsActive);

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, $"api/organizations/{organizationId:D}")
        {
            Content = JsonContent.Create(payload)
        };
        using var _ = await _backendClient.SendAsync(putRequest, ct);

        return await GetOrganizationOverviewAsync(organizationId, ct);
    }

    public async Task<MyOrganizationsViewModel> GetMyOrganizationsAsync(CancellationToken ct = default)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetMyOrganizationsResponse>(
            "api/users/me/organizations",
            ct) ?? new GetMyOrganizationsResponse([]);

        var cards = payload.Items
            .Select(x => new MyOrganizationCardViewModel
            {
                OrganizationId = x.OrganizationId,
                Name = x.OrganizationName,
                ShortDescription = x.OrganizationDescription,
                AvatarUrl = x.OrganizationAvatarUrl,
                MembershipRole = string.IsNullOrWhiteSpace(x.MemberRole) ? "Member" : x.MemberRole,
                MembershipStatus = "ACTIVE",
                MemberCount = 0,
                UpcomingEventCount = 0,
                ActiveTaskCount = 0,
                JoinedAtUtc = x.JoinedAtUtc,
                LastActivityAtUtc = null,
                Tags = [],
                CanAccessWorkspace = true,
                CanManage = HasOverviewWritePermission(x.MemberRole)
            })
            .OrderByDescending(x => x.JoinedAtUtc)
            .ToList();

        return new MyOrganizationsViewModel
        {
            LeadingOrganizations = cards.Where(x => x.CanManage).ToList(),
            ParticipatingOrganizations = cards.Where(x => !x.CanManage).ToList()
        };
    }

    public async Task<OrganizationViewerPermissionViewModel> GetOrganizationViewerPermissionAsync(Guid organizationId, CancellationToken ct = default)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetOrganizationPermissionsMeResponse>(
            $"api/organizations/{organizationId:D}/permissions/me",
            ct) ?? new GetOrganizationPermissionsMeResponse(new OrganizationPermissionDto(
                false, false, false, false, false, false, false, false, false, false, null, []));

        var data = payload.Data;
        return new OrganizationViewerPermissionViewModel
        {
            IsAuthenticated = data.IsAuthenticated,
            IsMember = data.IsMember,
            CanAccessWorkspace = data.CanAccessWorkspace,
            CanEditOverview = data.CanEditOverview,
            ViewerMode = data.IsMember
                ? (data.CanEditOverview ? "INTERNAL_WRITE" : "INTERNAL_READ")
                : "EXTERNAL",
            MemberRole = data.MemberRole
        };
    }

    public async Task<OrganizationDetailViewModel> CreateOrganizationAsync(CreateOrganizationViewModel model, CancellationToken ct = default)
    {
        var payload = new CreateOrganizationRequest(
            model.Name.Trim(),
            model.Description,
            model.AvatarUrl,
            model.CoverUrl,
            DateOnly.FromDateTime(DateTime.Today),
            model.Location);

        var created = await _backendClient.PostAsJsonAsync<CreateOrganizationRequest, OrganizationDto>(
            "api/organizations",
            payload,
            ct) ?? throw new InvalidOperationException("Backend returned empty organization payload.");

        return new OrganizationDetailViewModel
        {
            Id = created.Id,
            Name = created.Name,
            Description = created.Description ?? string.Empty,
            AvatarUrl = created.AvatarUrl,
            CoverUrl = created.CoverUrl,
            Location = created.Location,
            TotalMembers = created.TotalMembers,
            IsActive = created.IsActive,
            FoundedDate = created.FoundingDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today
        };
    }

    private static bool HasOverviewWritePermission(string? roleName)
        => roleName?.Trim().ToUpperInvariant() is "PRESIDENT" or "VICEPRESIDENT" or "MANAGER" or "OWNER" or "ADMIN";

    private static bool IsLeadershipRole(MemberRole role)
        => role is MemberRole.President or MemberRole.VicePresident or MemberRole.Manager;

    private static int GetRoleRank(MemberRole role)
    {
        return role switch
        {
            MemberRole.President => 0,
            MemberRole.VicePresident => 1,
            MemberRole.Manager => 2,
            _ => 9
        };
    }
}
