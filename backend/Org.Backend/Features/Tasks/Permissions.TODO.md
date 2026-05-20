# Tasks Module Permissions

## Permission Requirements by Endpoint

### POST /api/categories/{categoryId}/tasks
- **Permission**: `org.events.manage`

### GET /api/tasks/{taskId}
- **Permission**: `org.events.view` OR `org.events.manage`

### PUT /api/tasks/{taskId}
- **Permission**: `org.events.manage`

### DELETE /api/tasks/{taskId}
- **Permission**: `org.events.manage`

### PUT /api/tasks/{taskId}/status
- **Permission**: `org.events.manage`

### PUT /api/tasks/{taskId}/assign
- **Permission**: `org.events.manage`

## Permission Keys
- `org.events.view`
- `org.events.manage`
- `org.workspace.access`

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
