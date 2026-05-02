using System.Net;
using System.Net.Http.Json;
using Org.Shared.Features.Events;
using Org.Shared.Features.Organizations;
using Org.Shared.Features.Users;

namespace Org.Frontend.Services.Organizations;

public sealed class OrganizationServiceApiClient(HttpClient httpClient) : IOrganizationService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<OrganizationOverviewViewModel> GetOrganizationOverviewAsync(Guid organizationId, CancellationToken ct = default)
    {
        var permission = await GetOrganizationViewerPermissionAsync(organizationId, ct);
        var publicPayload = await _httpClient.GetFromJsonAsync<GetPublicOrganizationOverviewResponse>(
            $"api/organizations/{organizationId:D}/public-overview",
            cancellationToken: ct)
            ?? throw new InvalidOperationException("Backend returned empty public organization payload.");
        var dto = publicPayload.Data;

        var eventCount = 0;
        var upcomingEventCount = 0;
        if (permission.IsMember)
        {
            var eventsPayload = await _httpClient.GetFromJsonAsync<GetOrganizationEventsResponse>(
                $"api/organizations/{organizationId:D}/events",
                cancellationToken: ct);

            if (eventsPayload is not null)
            {
                eventCount = eventsPayload.Items.Count;
                var today = DateOnly.FromDateTime(DateTime.Today);
                upcomingEventCount = eventsPayload.Items.Count(x => x.StartDate >= today);
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
            ActiveTaskCount = 0,
            MilestoneCount = 0,
            LastActivityAtUtc = dto.UpdatedAtUtc?.UtcDateTime ?? dto.CreatedAtUtc.UtcDateTime,
            ViewerPermission = permission,
            Departments = [],
            Timeline = [],
            HighlightEvents = [],
            Leadership = []
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

        var current = await _httpClient.GetFromJsonAsync<GetOrganizationByIdResponse>(
            $"api/organizations/{organizationId:D}",
            cancellationToken: ct)
            ?? throw new InvalidOperationException("Backend returned empty organization payload.");

        var payload = new UpdateOrganizationRequest(
            request.Name.Trim(),
            request.Description,
            request.AvatarUrl,
            request.CoverUrl,
            current.Data.FoundingDate,
            request.Location,
            current.Data.IsActive);

        using var response = await _httpClient.PutAsJsonAsync($"api/organizations/{organizationId:D}", payload, ct);
        response.EnsureSuccessStatusCode();

        return await GetOrganizationOverviewAsync(organizationId, ct);
    }

    public async Task<MyOrganizationsViewModel> GetMyOrganizationsAsync(CancellationToken ct = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetMyOrganizationsResponse>(
            "api/users/me/organizations",
            cancellationToken: ct) ?? new GetMyOrganizationsResponse([]);

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
        var payload = await _httpClient.GetFromJsonAsync<GetOrganizationPermissionsMeResponse>(
            $"api/organizations/{organizationId:D}/permissions/me",
            cancellationToken: ct)
            ?? new GetOrganizationPermissionsMeResponse(new OrganizationPermissionDto(
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

        using var response = await _httpClient.PostAsJsonAsync("api/organizations", payload, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<OrganizationDto>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Backend returned empty organization payload.");

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
}
