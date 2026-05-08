# RolesPermissions Module Permissions

## Permission Requirements by Endpoint

### GET /api/organizations/{orgId}/permissions/me
- **Permission**: `org.workspace.access` (member of organization)
- **Notes**: Returns user's own permissions in organization

### GET /api/organizations/{orgId}/permissions
- **Permission**: `org.roles.view`

### GET /api/organizations/{orgId}/roles
- **Permission**: `org.roles.view`

### POST /api/organizations/{orgId}/roles
- **Permission**: `org.roles.create`

### PUT /api/organizations/roles/{roleId}
- **Permission**: `org.roles.update`

### DELETE /api/organizations/roles/{roleId}
- **Permission**: `org.roles.delete`

### POST /api/organizations/{orgId}/members/{memberId}/role
- **Permission**: `org.roles.assign`

## Permission Keys
- `org.roles.view`
- `org.roles.create`
- `org.roles.update`
- `org.roles.delete`
- `org.roles.assign`
- `org.workspace.access`

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
