# Milestones Module Permissions

## Permission Requirements by Endpoint

### GET /api/events/{eventId}/milestones
- **Permission**: `org.events.view` OR `org.workspace.access`

### POST /api/events/{eventId}/milestones
- **Permission**: `org.events.manage`

### GET /api/milestones/{id}
- **Permission**: `org.events.view` OR `org.events.manage`

### PUT /api/milestones/{id}
- **Permission**: `org.events.manage`

### DELETE /api/milestones/{id}
- **Permission**: `org.events.manage`

## Permission Keys
- `org.events.view`
- `org.events.manage`
- `org.workspace.access`

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
