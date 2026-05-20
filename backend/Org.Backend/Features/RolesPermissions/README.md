# RolesPermissions Module

## Module Purpose
Role and permission management within organizations including role CRUD and member role assignment.

## Scope Status
**CORE** - Full backend skeleton required

## Related Domain Entities
- `Role`, `Permission`, `RolePermission`, `Member`, `Organization`
- Enums: `MemberRole` (for hierarchy/default mapping only)

## Expected Backend Routes
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/organizations/{orgId}/permissions/me` | Get current user permissions in org |
| GET | `/api/organizations/{orgId}/permissions` | List all permissions |
| GET | `/api/organizations/{orgId}/roles` | List organization roles |
| POST | `/api/organizations/{orgId}/roles` | Create custom role |
| PUT | `/api/organizations/roles/{roleId}` | Update role |
| DELETE | `/api/organizations/roles/{roleId}` | Delete role |
| POST | `/api/organizations/{orgId}/members/{memberId}/role` | Assign role to member |

## Required Permissions
- `org.roles.view` - View roles
- `org.roles.create` - Create roles
- `org.roles.update` - Update roles
- `org.roles.delete` - Delete roles
- `org.roles.assign` - Assign roles to members

## Important Notes
- RoleId is canonical source of truth for member roles
- MemberRole enum is NOT persisted in Member entity
- permissions/me response must be normalizable to string[]
- Permission fallback must NEVER grant org.workspace.access
- Role assignment belongs here, NOT in Members module

## Cross-layer Contract Notes
- Future contract: `backend/Org.Shared/Features/RolesPermissions/RoleContracts.cs.TODO`
- Future service: `frontend/org-frontend/src/services/roleService.js`
- Future adapter: `frontend/org-frontend/src/adapters/roleAdapter.js`
- Future page: `OrgRolesPage.jsx`
- Permissions: `org.roles.view`, `org.roles.create`, `org.roles.update`, `org.roles.delete`, `org.roles.assign`
- Status: **CORE**
