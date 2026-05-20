# Organizations Module Permissions

## Permission Requirements by Endpoint

### GET /api/organizations
- **Permission**: Valid JWT token
- **Notes**: Returns organizations where user is a member

### POST /api/organizations
- **Permission**: Valid JWT token
- **Notes**: Any authenticated user can create organization

### GET /api/organizations/default
- **Permission**: Valid JWT token
- **Notes**: Returns user's first/default organization

### GET /api/organizations/{id}
- **Permission**: `org.overview.read` OR `org.workspace.access`
- **Notes**: User must be member of organization

### PUT /api/organizations/{id}
- **Permission**: `org.overview.write`
- **Notes**: User must have write permission

### GET /api/organizations/{id}/public-overview
- **Permission**: None (public endpoint)
- **Notes**: Must not fail if permissions/me returns 403

## Permission Keys
- `org.overview.read`
- `org.overview.write`
- `org.workspace.access`

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
