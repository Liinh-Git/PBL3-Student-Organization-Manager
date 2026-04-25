// ---- API client cho dashboard user: tổng hợp tổ chức tham gia và sự kiện đã ghi danh ----
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.ViewModels;
using Org.Shared;
using Org.Shared.Features.Users;

namespace Org.Frontend.Services.Dashboard;

public sealed class UserDashboardApiClient(
    HttpClient httpClient,
    AuthenticationStateProvider authStateProvider) : IUserDashboardService
{
    private const string DefaultOrganizationImageUrl = "/images/mockimages/1773043475804.png";
    private const string DefaultEventImageUrl = "/images/mockimages/IMG_20260301_060428.jpg";

    private readonly HttpClient _httpClient = httpClient;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<UserDashboardViewModel> GetDashboardAsync(CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var organizationsPayload = await _httpClient.GetFromJsonAsync<GetMyOrganizationsResponse>(
            "api/users/me/organizations",
            cancellationToken: ct) ?? new GetMyOrganizationsResponse([]);

        var eventsPayload = await _httpClient.GetFromJsonAsync<GetMyRegisteredEventsResponse>(
            "api/users/me/events",
            cancellationToken: ct) ?? new GetMyRegisteredEventsResponse([]);

        var suggestedOrganizationsPayload = await _httpClient.GetFromJsonAsync<GetSuggestedOrganizationsResponse>(
            "api/users/me/discover/organizations",
            cancellationToken: ct) ?? new GetSuggestedOrganizationsResponse([]);

        var suggestedEventsPayload = await _httpClient.GetFromJsonAsync<GetSuggestedEventsResponse>(
            "api/users/me/discover/events",
            cancellationToken: ct) ?? new GetSuggestedEventsResponse([]);

        var myOrganizations = organizationsPayload.Items
            .OrderByDescending(x => x.JoinedAtUtc)
            .ThenBy(x => x.OrganizationName, StringComparer.OrdinalIgnoreCase)
            .Select(x => new UserOrganizationViewModel
            {
                OrganizationId = x.OrganizationId,
                Name = x.OrganizationName,
                Description = x.OrganizationDescription,
                AvatarUrl = string.IsNullOrWhiteSpace(x.OrganizationAvatarUrl)
                    ? DefaultOrganizationImageUrl
                    : x.OrganizationAvatarUrl,
                JoinedAtUtc = x.JoinedAtUtc,
                Role = string.IsNullOrWhiteSpace(x.MemberRole) ? "Member" : x.MemberRole
            })
            .ToList();

        var organizationImageLookup = myOrganizations.ToDictionary(
            x => x.OrganizationId,
            x => string.IsNullOrWhiteSpace(x.AvatarUrl) ? DefaultOrganizationImageUrl : x.AvatarUrl!);

        return new UserDashboardViewModel
        {
            DisplayName = user.Identity?.Name?.Trim() ?? "Nguoi dung",
            Email = user.FindFirst(ClaimTypes.Email)?.Value,
            Organizations = myOrganizations,
            RegisteredEvents = eventsPayload.Items
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.EventName, StringComparer.OrdinalIgnoreCase)
                .Select(x => new UserRegisteredEventViewModel
                {
                    EventId = x.EventId,
                    OrganizationId = x.OrganizationId,
                    OrganizationName = x.OrganizationName,
                    Name = x.EventName,
                    Description = x.EventDescription,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    EventStatus = ToStatusCode(x.EventStatus),
                    RegistrationStatus = NormalizeRegistrationStatus(x.RegistrationStatus),
                    RegisteredAtUtc = x.RegisteredAtUtc,
                    Location = x.Location,
                    ImageUrl = ResolveRegisteredEventImage(x.EventImageUrl, x.OrganizationId, organizationImageLookup)
                })
                .ToList(),
            SuggestedOrganizations = suggestedOrganizationsPayload.Items
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.TotalMembers)
                .ThenBy(x => x.OrganizationName, StringComparer.OrdinalIgnoreCase)
                .Select(x => new SuggestedOrganizationViewModel
                {
                    OrganizationId = x.OrganizationId,
                    Name = x.OrganizationName,
                    Description = x.OrganizationDescription,
                    ImageUrl = string.IsNullOrWhiteSpace(x.OrganizationImageUrl) ? DefaultOrganizationImageUrl : x.OrganizationImageUrl,
                    MemberCount = x.TotalMembers,
                    Location = x.Location,
                    IsActive = x.IsActive
                })
                .ToList(),
            SuggestedEvents = suggestedEventsPayload.Items
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.EventName, StringComparer.OrdinalIgnoreCase)
                .Select(x => new SuggestedEventViewModel
                {
                    EventId = x.EventId,
                    OrganizationId = x.OrganizationId,
                    OrganizationName = x.OrganizationName,
                    Name = x.EventName,
                    Description = x.EventDescription,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    EventStatus = ToStatusCode(x.EventStatus),
                    Location = x.Location,
                    ImageUrl = string.IsNullOrWhiteSpace(x.EventImageUrl) ? DefaultEventImageUrl : x.EventImageUrl,
                    RegisteredCount = x.RegisteredCount
                })
                .ToList()
        };
    }

    private static string ResolveRegisteredEventImage(
        string? eventImageUrl,
        Guid organizationId,
        IReadOnlyDictionary<Guid, string> organizationImageLookup)
    {
        if (!string.IsNullOrWhiteSpace(eventImageUrl))
            return eventImageUrl;

        return organizationImageLookup.TryGetValue(organizationId, out var imageUrl)
            ? imageUrl
            : DefaultEventImageUrl;
    }

    private static string ToStatusCode(EventStatus status)
        => status switch
        {
            EventStatus.Ongoing => "ONGOING",
            EventStatus.Completed => "COMPLETED",
            EventStatus.Planning => "PLANNING",
            _ => "DRAFT"
        };

    private static string NormalizeRegistrationStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "ATTENDED" => "ATTENDED",
            "CANCELLED" => "CANCELLED",
            _ => "REGISTERED"
        };
    }
}
