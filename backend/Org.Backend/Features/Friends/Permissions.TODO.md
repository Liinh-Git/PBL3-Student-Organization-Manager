# Friends Module Permissions

## Permission Requirements by Endpoint

### GET /api/friends
- **Permission**: Valid JWT token
- **Notes**: User can only view their own friends

### GET /api/friends/requests
- **Permission**: Valid JWT token
- **Notes**: User can only view their own friend requests

### POST /api/friends/requests
- **Permission**: Valid JWT token
- **Notes**: User can send friend requests to other users

### POST /api/friends/requests/{id}/accept
- **Permission**: Valid JWT token
- **Notes**: User can only accept requests sent to them

### POST /api/friends/requests/{id}/reject
- **Permission**: Valid JWT token
- **Notes**: User can only reject requests sent to them

## Permission Keys
No specific organization permission keys required (user-scoped endpoints)

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
