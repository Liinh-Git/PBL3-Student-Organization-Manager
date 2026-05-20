# Members Module Permissions

## Permission Requirements by Endpoint

### GET /api/organizations/{orgId}/members
- **Permission**: `org.members.view` OR `org.workspace.access`

### POST /api/organizations/{orgId}/members
- **Permission**: `org.members.manage`

### PUT /api/members/{id}/department
- **Permission**: `org.members.manage`

### DELETE /api/members/{id}
- **Permission**: `org.members.manage`

## Permission Keys
- `org.members.view`
- `org.members.manage`
- `org.workspace.access`

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
