# RolesPermissions Services

## IRoleService / RoleService
**Methods**:
- `Task<List<string>> GetMyPermissionsAsync(Guid orgId, Guid userId)`
- `Task<List<PermissionDto>> ListPermissionsAsync(Guid orgId, Guid userId)`
- `Task<List<RoleDto>> ListRolesAsync(Guid orgId, Guid userId)`
- `Task<RoleDto> CreateRoleAsync(Guid orgId, CreateRoleRequest request, Guid userId)`
- `Task<RoleDto> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, Guid userId)`
- `Task DeleteRoleAsync(Guid roleId, Guid userId)`
- `Task AssignRoleToMemberAsync(Guid orgId, Guid memberId, Guid roleId, Guid userId)`

## NOT Implemented in Phase 3C
- ❌ No real service implementations
