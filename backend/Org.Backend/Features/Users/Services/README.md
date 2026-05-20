# Users Services

## Overview
Service layer for user profile and settings business logic.

## Planned Services

### IUserService / UserService
**Methods**:
- `Task<UserProfileDto> GetMyProfileAsync(Guid userId)`
- `Task<UserProfileDto> UpdateMyProfileAsync(Guid userId, UpdateProfileRequest request)`
- `Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)`
- `Task<List<UserOrganizationDto>> GetMyOrganizationsAsync(Guid userId)`
- `Task<List<UserEventDto>> GetMyEventsAsync(Guid userId)`
- `Task<List<OrganizationDto>> GetDiscoverOrganizationsAsync(Guid userId)`

## Implementation Notes
- Query User with related entities (Members, Organizations, Events)
- Password change requires verification of current password
- Profile visibility affects data returned
- getMyOrganizations belongs to userService, NOT organizationService

## NOT Implemented in Phase 3C
- ❌ No real service implementations
- ❌ Only README with structure notes
