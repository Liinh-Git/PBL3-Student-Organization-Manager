# Users Module Permissions

## Overview
User module endpoints are user-scoped and require valid JWT token but no specific organization permissions.

## Permission Requirements by Endpoint

### GET /api/users/me
- **Permission**: Valid JWT token only
- **Authorization**: Authenticated user
- **Notes**: User can always view their own profile

### PUT /api/users/me
- **Permission**: Valid JWT token only
- **Authorization**: Authenticated user
- **Notes**: User can always update their own profile

### PUT /api/users/me/change-password
- **Permission**: Valid JWT token only
- **Authorization**: Authenticated user
- **Notes**: User can always change their own password

### GET /api/users/me/organizations
- **Permission**: Valid JWT token only
- **Authorization**: Authenticated user
- **Notes**: User can view their own organizations

### GET /api/users/me/events
- **Permission**: Valid JWT token only
- **Authorization**: Authenticated user
- **Notes**: User can view their own events

### GET /api/users/me/discover/organizations
- **Permission**: Valid JWT token only
- **Authorization**: Authenticated user
- **Notes**: User can discover public organizations

## Permission Keys
No specific organization permission keys required for user-scoped endpoints.

## NOT Implemented in Phase 3C
- ❌ No permission checking logic
- ❌ Only documentation of permission requirements
