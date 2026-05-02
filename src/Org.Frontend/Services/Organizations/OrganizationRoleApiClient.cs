using System.Net.Http.Json;
using Org.Frontend.ViewModels;
using Org.Shared.Features.Organizations;
using RoleUpsertRequest = Org.Frontend.ViewModels.UpsertOrganizationRoleRequest;

namespace Org.Frontend.Services.Organizations;

public sealed class OrganizationRoleApiClient(HttpClient httpClient) : IOrganizationRoleService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<bool> CanManageRolesAsync(Guid organizationId, CancellationToken ct = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetOrganizationPermissionsMeResponse>(
            $"api/organizations/{organizationId:D}/permissions/me",
            cancellationToken: ct);

        return payload?.Data.CanManageRoles ?? false;
    }

    public async Task<IReadOnlyList<PermissionOptionViewModel>> GetAvailablePermissionsAsync(Guid organizationId, CancellationToken ct = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetOrganizationPermissionsCatalogResponse>(
            $"api/organizations/{organizationId:D}/permissions",
            cancellationToken: ct) ?? new GetOrganizationPermissionsCatalogResponse([]);

        return payload.Items
            .Select(x => new PermissionOptionViewModel
            {
                Code = x.Code,
                Group = x.Group,
                Label = x.Label
            })
            .ToList();
    }

    public async Task<IReadOnlyList<OrganizationRoleViewModel>> GetRolesAsync(Guid organizationId, CancellationToken ct = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<GetOrganizationRolesResponse>(
            $"api/organizations/{organizationId:D}/roles",
            cancellationToken: ct) ?? new GetOrganizationRolesResponse([]);

        return payload.Items
            .Select(x => new OrganizationRoleViewModel
            {
                Id = x.Id,
                OrganizationId = x.OrganizationId,
                Name = x.Name,
                Description = x.Description,
                IsProtected = x.IsProtected,
                AssignedMemberCount = x.AssignedMemberCount,
                PermissionCodes = x.PermissionCodes
            })
            .ToList();
    }

    public async Task<OrganizationRoleViewModel> CreateRoleAsync(Guid organizationId, RoleUpsertRequest request, CancellationToken ct = default)
    {
        var payload = new Org.Shared.Features.Organizations.UpsertOrganizationRoleRequest(
            request.Name.Trim(),
            request.Description,
            request.PermissionCodes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList());

        using var response = await _httpClient.PostAsJsonAsync(
            $"api/organizations/{organizationId:D}/roles",
            payload,
            ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<OrganizationRoleDto>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Backend returned empty role payload.");

        return MapRole(created);
    }

    public async Task<OrganizationRoleViewModel> UpdateRoleAsync(Guid roleId, RoleUpsertRequest request, CancellationToken ct = default)
    {
        var payload = new Org.Shared.Features.Organizations.UpsertOrganizationRoleRequest(
            request.Name.Trim(),
            request.Description,
            request.PermissionCodes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList());

        using var response = await _httpClient.PutAsJsonAsync(
            $"api/organizations/roles/{roleId:D}",
            payload,
            ct);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<OrganizationRoleDto>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Backend returned empty role payload.");

        return MapRole(updated);
    }

    public async Task DeleteRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        using var response = await _httpClient.DeleteAsync($"api/organizations/roles/{roleId:D}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task AssignRoleToMemberAsync(Guid organizationId, Guid memberId, Guid roleId, CancellationToken ct = default)
    {
        var payload = new AssignOrganizationRoleRequest(roleId);
        using var response = await _httpClient.PostAsJsonAsync(
            $"api/organizations/{organizationId:D}/members/{memberId:D}/role",
            payload,
            ct);
        response.EnsureSuccessStatusCode();
    }

    private static OrganizationRoleViewModel MapRole(OrganizationRoleDto role)
    {
        return new OrganizationRoleViewModel
        {
            Id = role.Id,
            OrganizationId = role.OrganizationId,
            Name = role.Name,
            Description = role.Description,
            IsProtected = role.IsProtected,
            AssignedMemberCount = role.AssignedMemberCount,
            PermissionCodes = role.PermissionCodes
        };
    }
}
