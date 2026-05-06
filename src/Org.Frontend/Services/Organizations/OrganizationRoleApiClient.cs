using System.Net.Http.Json;
using Org.Frontend.Services.Auth;
using Org.Frontend.ViewModels;
using Org.Shared.Features.Organizations;
using RoleUpsertRequest = Org.Frontend.ViewModels.UpsertOrganizationRoleRequest;

namespace Org.Frontend.Services.Organizations;

public sealed class OrganizationRoleApiClient(IAuthenticatedBackendClient backendClient) : IOrganizationRoleService
{
    private readonly IAuthenticatedBackendClient _backendClient = backendClient;

    public async Task<bool> CanManageRolesAsync(Guid organizationId, CancellationToken ct = default)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetOrganizationPermissionsMeResponse>(
            $"api/organizations/{organizationId:D}/permissions/me",
            ct);

        return payload?.Data.CanManageRoles ?? false;
    }

    public async Task<IReadOnlyList<PermissionOptionViewModel>> GetAvailablePermissionsAsync(Guid organizationId, CancellationToken ct = default)
    {
        var payload = await _backendClient.GetFromJsonAsync<GetOrganizationPermissionsCatalogResponse>(
            $"api/organizations/{organizationId:D}/permissions",
            ct) ?? new GetOrganizationPermissionsCatalogResponse([]);

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
        var payload = await _backendClient.GetFromJsonAsync<GetOrganizationRolesResponse>(
            $"api/organizations/{organizationId:D}/roles",
            ct) ?? new GetOrganizationRolesResponse([]);

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

        var created = await _backendClient.PostAsJsonAsync<Org.Shared.Features.Organizations.UpsertOrganizationRoleRequest, OrganizationRoleDto>(
            $"api/organizations/{organizationId:D}/roles",
            payload,
            ct) ?? throw new InvalidOperationException("Backend returned empty role payload.");

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

        var updated = await _backendClient.PutAsJsonAsync<Org.Shared.Features.Organizations.UpsertOrganizationRoleRequest, OrganizationRoleDto>(
            $"api/organizations/roles/{roleId:D}",
            payload,
            ct) ?? throw new InvalidOperationException("Backend returned empty role payload.");

        return MapRole(updated);
    }

    public async Task DeleteRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        await _backendClient.DeleteAsync($"api/organizations/roles/{roleId:D}", ct);
    }

    public async Task AssignRoleToMemberAsync(Guid organizationId, Guid memberId, Guid roleId, CancellationToken ct = default)
    {
        var payload = new AssignOrganizationRoleRequest(roleId);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/organizations/{organizationId:D}/members/{memberId:D}/role")
        {
            Content = JsonContent.Create(payload)
        };

        using var _ = await _backendClient.SendAsync(request, ct);
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
