# EventCategories Module Permissions

## Permission Requirements by Endpoint

### GET /api/milestones/{milestoneId}/categories
- **Permission**: `org.events.view` OR `org.workspace.access`

### POST /api/milestones/{milestoneId}/categories
- **Permission**: `org.events.manage`

### GET /api/categories/{id}
- **Permission**: `org.events.view` OR `org.events.manage`

### PUT /api/categories/{id}
- **Permission**: `org.events.manage`

### DELETE /api/categories/{id}
- **Permission**: `org.events.manage`

## Permission Keys
- `org.events.view`
- `org.events.manage`
- `org.workspace.access`

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
