using Microsoft.AspNetCore.WebUtilities;
using Org.Frontend.Services.Dashboard;

namespace Org.Frontend.Services.Organizations;

public interface IOrganizationRouteAccessGate
{
    bool HasRestrictedPublicOrganization { get; }
    Guid? RestrictedOrganizationId { get; }
    Task SyncFromUriAsync(string absoluteUri, CancellationToken ct = default);
    bool ShouldBlockInternalRoute(string route);
    bool ShouldUseOrganizationWorkspace(string route);
    string BuildRestrictedOverviewRoute();
}

public sealed class OrganizationRouteAccessGate(IUserDashboardService dashboardService) : IOrganizationRouteAccessGate
{
    private readonly IUserDashboardService _dashboardService = dashboardService;
    private HashSet<Guid>? _joinedOrganizationIds;

    private Guid? _publicOrganizationId;
    private bool _isPublicOrganizationMember = true;

    public bool HasRestrictedPublicOrganization =>
        _publicOrganizationId.HasValue && !_isPublicOrganizationMember;

    public Guid? RestrictedOrganizationId => HasRestrictedPublicOrganization
        ? _publicOrganizationId
        : null;

    public async Task SyncFromUriAsync(string absoluteUri, CancellationToken ct = default)
    {
        var route = GetRouteFromUri(absoluteUri);

        if (IsOrganizationOverviewRoute(route))
        {
            if (TryGetOrgIdFromUri(absoluteUri, out var orgId))
            {
                _publicOrganizationId = orgId;
                _isPublicOrganizationMember = await IsMemberAsync(orgId, ct);
                return;
            }

            _publicOrganizationId = null;
            _isPublicOrganizationMember = true;
            return;
        }

        if (IsInternalOrganizationRoute(route))
        {
            // Keep the last public org context so deep-links can be blocked
            // while the user is viewing an org they have not joined.
            return;
        }

        _publicOrganizationId = null;
        _isPublicOrganizationMember = true;
    }

    public bool ShouldBlockInternalRoute(string route)
    {
        return IsInternalOrganizationRoute(route) && HasRestrictedPublicOrganization;
    }

    public bool ShouldUseOrganizationWorkspace(string route)
    {
        if (IsInternalOrganizationRoute(route))
        {
            return !HasRestrictedPublicOrganization;
        }

        if (IsOrganizationOverviewRoute(route))
        {
            if (RestrictedOrganizationId.HasValue)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public string BuildRestrictedOverviewRoute()
    {
        return RestrictedOrganizationId.HasValue
            ? $"/org-overview?orgId={RestrictedOrganizationId.Value}"
            : "/home";
    }

    private async Task<bool> IsMemberAsync(Guid organizationId, CancellationToken ct)
    {
        if (_joinedOrganizationIds is null)
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardAsync(ct);
                _joinedOrganizationIds = dashboard.Organizations
                    .Select(x => x.OrganizationId)
                    .ToHashSet();
            }
            catch
            {
                _joinedOrganizationIds = [];
            }
        }

        return _joinedOrganizationIds.Contains(organizationId);
    }

    private static bool IsInternalOrganizationRoute(string route)
        => route.StartsWith("/org/", StringComparison.OrdinalIgnoreCase);

    private static bool IsOrganizationOverviewRoute(string route)
        => route.StartsWith("/org-overview", StringComparison.OrdinalIgnoreCase);

    private static string GetRouteFromUri(string absoluteUri)
    {
        if (!Uri.TryCreate(absoluteUri, UriKind.Absolute, out var uri))
        {
            return "/";
        }

        var path = uri.AbsolutePath.Trim('/');
        return string.IsNullOrWhiteSpace(path) ? "/" : "/" + path;
    }

    private static bool TryGetOrgIdFromUri(string absoluteUri, out Guid organizationId)
    {
        organizationId = Guid.Empty;

        if (!Uri.TryCreate(absoluteUri, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var query = QueryHelpers.ParseQuery(uri.Query);
        if (!query.TryGetValue("orgId", out var rawOrgId))
        {
            return false;
        }

        return Guid.TryParse(rawOrgId.ToString(), out organizationId);
    }
}