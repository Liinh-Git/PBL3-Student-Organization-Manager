# Events Module Permissions

## Permission Requirements by Endpoint

### GET /api/organizations/{orgId}/events
- **Permission**: `org.events.view` OR `org.workspace.access`

### POST /api/organizations/{orgId}/events
- **Permission**: `org.events.create`

### GET /api/events/{id}
- **Permission**: `org.events.view` OR `org.events.manage`

### PUT /api/events/{id}
- **Permission**: `org.events.manage`

### DELETE /api/events/{id}
- **Permission**: `org.events.manage`

### GET /api/events/public
- **Permission**: None (public endpoint)

### GET /api/events/{id}/public
- **Permission**: None (public endpoint)

## Permission Keys
- `org.events.view`
- `org.events.create`
- `org.events.manage`
- `org.workspace.access`

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
