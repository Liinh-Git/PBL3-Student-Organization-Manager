# Departments Module Permissions

## Permission Requirements by Endpoint

### GET /api/organizations/{orgId}/departments
- **Permission**: `org.departments.view` OR `org.workspace.access`

### POST /api/organizations/{orgId}/departments
- **Permission**: `org.departments.manage`

### GET /api/departments/{id}
- **Permission**: `org.departments.view` OR `org.workspace.access`

### PUT /api/departments/{id}
- **Permission**: `org.departments.manage`

### DELETE /api/departments/{id}
- **Permission**: `org.departments.manage`

## Permission Keys
- `org.departments.view`
- `org.departments.manage`
- `org.workspace.access`

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
