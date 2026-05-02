using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Organizations;

public interface IOrganizationRoleService
{
    Task<bool> CanManageRolesAsync(Guid organizationId, CancellationToken ct = default);
    Task<IReadOnlyList<PermissionOptionViewModel>> GetAvailablePermissionsAsync(Guid organizationId, CancellationToken ct = default);
    Task<IReadOnlyList<OrganizationRoleViewModel>> GetRolesAsync(Guid organizationId, CancellationToken ct = default);
    Task<OrganizationRoleViewModel> CreateRoleAsync(Guid organizationId, UpsertOrganizationRoleRequest request, CancellationToken ct = default);
    Task<OrganizationRoleViewModel> UpdateRoleAsync(Guid roleId, UpsertOrganizationRoleRequest request, CancellationToken ct = default);
    Task DeleteRoleAsync(Guid roleId, CancellationToken ct = default);
    Task AssignRoleToMemberAsync(Guid organizationId, Guid memberId, Guid roleId, CancellationToken ct = default);
}
