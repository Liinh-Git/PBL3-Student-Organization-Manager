using Org.Shared.Features.RolesPermissions;

namespace Org.Backend.Features.RolesPermissions.Services;

public interface IRoleService
{
    // Read operations
    Task<MyPermissionsResponse> GetMyPermissionsAsync(Guid orgId, Guid userId, CancellationToken ct = default);
    Task<List<RoleDto>> GetOrganizationRolesAsync(Guid orgId, Guid userId, CancellationToken ct = default);
    
    // Write operations
    Task<RoleDto> CreateRoleAsync(Guid orgId, Guid userId, CreateRoleRequest request, CancellationToken ct = default);
    Task<RoleDto> UpdateRoleAsync(Guid roleId, Guid userId, UpdateRoleRequest request, CancellationToken ct = default);
    Task<bool> DeleteRoleAsync(Guid roleId, Guid userId, CancellationToken ct = default);
    Task<bool> AssignRoleToMemberAsync(Guid orgId, Guid memberId, Guid userId, AssignRoleToMemberRequest request, CancellationToken ct = default);
}
