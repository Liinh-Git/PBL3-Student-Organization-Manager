# Requests Module Permissions

## Permission Requirements by Endpoint

### GET /api/organizations/{orgId}/requests
- **Permission**: `org.requests.view`

### POST /api/organizations/{orgId}/requests
- **Permission**: Valid JWT token (any authenticated user can submit)

### GET /api/requests/{requestId}
- **Permission**: `org.requests.view` OR request sender

### POST /api/organizations/requests/{requestId}/review
- **Permission**: `org.requests.review` AND `org.requests.approve`

## Permission Keys
- `org.requests.view`
- `org.requests.review`
- `org.requests.approve`

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
