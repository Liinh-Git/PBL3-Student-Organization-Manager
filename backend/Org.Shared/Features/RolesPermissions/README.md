# RolesPermissions Module Contracts

## Module Purpose
Role and permission management (role CRUD, permission gating, role assignment).

## Scope Status
**CORE** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/RolesPermissions/`

## Related Domain Entities
- `Role`, `Permission`, `RolePermission`, `Member`, `Organization`, `MemberRole` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/organizations/{orgId}/permissions/me` | JWT | None | `ApiResponse<MyPermissionsResponse>` |
| GET | `/api/organizations/{orgId}/permissions` | org.roles.view | None | `ApiResponse<ListResponse<PermissionDto>>` |
| GET | `/api/organizations/{orgId}/roles` | org.roles.view | None | `ApiResponse<ListResponse<RoleDto>>` |
| POST | `/api/organizations/{orgId}/roles` | org.roles.create | `CreateRoleRequest` | `ApiResponse<RoleDto>` |
| PUT | `/api/organizations/roles/{roleId}` | org.roles.update | `UpdateRoleRequest` | `ApiResponse<RoleDto>` |
| DELETE | `/api/organizations/roles/{roleId}` | org.roles.delete | None | `ApiResponse<bool>` |
| POST | `/api/organizations/{orgId}/members/{memberId}/role` | org.roles.assign | `AssignRoleToMemberRequest` | `ApiResponse<MemberDto>` |

## Future Request DTO Names
- `CreateRoleRequest`, `UpdateRoleRequest`, `AssignRoleToMemberRequest`

## Future Response DTO Names
- `PermissionDto`, `MyPermissionsResponse`, `RoleDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/roleService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/roleAdapter.js` (if needed)

## Future Page/Component Files
- `frontend/org-frontend/src/pages/org/OrgRolesPage.jsx`

## Required Permissions
- **Get my permissions**: JWT
- **List permissions**: org.roles.view
- **List roles**: org.roles.view
- **Create role**: org.roles.create
- **Update role**: org.roles.update
- **Delete role**: org.roles.delete
- **Assign role**: org.roles.assign

## Contract Notes

### MyPermissionsResponse
- **Fields**: `PermissionKeys` (string[])
- **Note**: Must normalize to string[] permissionKeys
- **Critical**: Fallback must NEVER grant org.workspace.access

### PermissionDto
- **Fields**: `Id`, `PermissionKey`, `DisplayName`, `ModuleGroup`, `Description?`
- **Note**: All available permissions in system

### RoleDto
- **Fields**: `Id`, `OrganizationId`, `RoleName`, `Description?`, `IsDefault`, `Level?`, `PermissionKeys` (string[])
- **Note**: Role with assigned permissions

### CreateRoleRequest
- **Fields**: `RoleName`, `Description?`, `PermissionKeys` (string[])
- **Validation**: RoleName required, unique per organization

### UpdateRoleRequest
- **Fields**: Same as CreateRoleRequest

### AssignRoleToMemberRequest
- **Fields**: `RoleId`
- **Note**: Assign role to member
- **Important**: RoleId is canonical, not fake role GUID

## Validation Notes
- **RoleName**: Required, max 100 characters, unique per organization
- **PermissionKeys**: Array of valid permission keys

## Mapping Notes
- **Entity → DTO**: Map `Role` entity to `RoleDto`, include permission keys
- **DTO → Entity**: Map request DTOs to `Role` entity

## What is NOT Implemented in This Phase
- ❌ No real role CRUD logic
- ❌ No real permission checking logic
- ❌ Only contract skeleton/TODO files

## Critical Notes
- **RoleId is canonical**, not MemberRole enum
- **permissions/me must normalize to string[]**
- **Fallback must NEVER grant org.workspace.access**

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/RolesPermissions/`
- **Shared Contract**: `backend/Org.Shared/Features/RolesPermissions/RoleContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/roleService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/roleAdapter.js`
- **Frontend Pages**: `OrgRolesPage.jsx`

---

**End of RolesPermissions README.md**
