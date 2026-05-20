# RolesPermissions Mappings

## Role Entity → RoleDto
- Map all role fields
- Include permission list (from RolePermissions)
- Include member count with this role

## Permission Entity → PermissionDto
- Map all permission fields
- Group by ModuleGroup

## Member Permissions → string[]
- Extract permission keys from Member.Role.RolePermissions
- Return flat array of permission key strings
- Normalize response format (handle various API response shapes)

## NOT Implemented in Phase 3C
- ❌ No real mapping implementations
