# Users Endpoints

## Overview
This folder will contain FastEndpoints endpoint implementations for user profile and settings management.

## Planned Endpoints

### 1. GetMyProfileEndpoint.cs.TODO
- **Route**: `GET /api/users/me`
- **Purpose**: Get current user profile
- **Request**: None (uses JWT)
- **Response**: `UserProfileDto`
- **Permission**: Valid JWT token

### 2. UpdateMyProfileEndpoint.cs.TODO
- **Route**: `PUT /api/users/me`
- **Purpose**: Update current user profile
- **Request**: `UpdateProfileRequest`
- **Response**: `UserProfileDto`
- **Permission**: Valid JWT token

### 3. ChangePasswordEndpoint.cs.TODO
- **Route**: `PUT /api/users/me/change-password`
- **Purpose**: Change user password
- **Request**: `ChangePasswordRequest`
- **Response**: Success message
- **Permission**: Valid JWT token

### 4. GetMyOrganizationsEndpoint.cs.TODO
- **Route**: `GET /api/users/me/organizations`
- **Purpose**: Get user's organizations
- **Request**: None (uses JWT)
- **Response**: `List<UserOrganizationDto>`
- **Permission**: Valid JWT token

### 5. GetMyEventsEndpoint.cs.TODO
- **Route**: `GET /api/users/me/events`
- **Purpose**: Get user's events
- **Request**: None (uses JWT)
- **Response**: `List<UserEventDto>`
- **Permission**: Valid JWT token

### 6. GetDiscoverOrganizationsEndpoint.cs.TODO
- **Route**: `GET /api/users/me/discover/organizations`
- **Purpose**: Get discoverable organizations for user
- **Request**: None (uses JWT)
- **Response**: `List<OrganizationDto>`
- **Permission**: Valid JWT token

### 7. GetMyTasksEndpoint.cs
- **Route**: `GET /api/users/me/tasks`
- **Purpose**: Get tasks assigned to current user with event/milestone/category context
- **Request**: Query `fromUtc`, `toUtc` (optional)
- **Response**: `List<MyTaskDto>`
- **Permission**: Valid JWT token

## Implementation Notes
- All endpoints require JWT authentication
- Extract UserId from JWT claims
- User-scoped endpoints do not require organization context
- Profile visibility affects what data is returned

## NOT Implemented in Phase 3C
- ❌ No real endpoint implementations
- ❌ Only `.TODO` files with structure notes
