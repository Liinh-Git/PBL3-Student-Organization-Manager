# Discover Module Permissions

## Permission Requirements by Endpoint

### GET /api/discover/organizations
- **Permission**: Valid JWT token
- **Notes**: Authenticated users can discover public organizations

### GET /api/discover/events
- **Permission**: Valid JWT token
- **Notes**: Authenticated users can discover public events

## Permission Keys
No specific organization permission keys required (user-scoped endpoints)

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
