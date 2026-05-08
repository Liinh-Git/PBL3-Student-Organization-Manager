# Users Module Contracts

## Module Purpose
User profile management, settings, and user-scoped data (my organizations, my events, discover organizations).

## Scope Status
**CORE** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/Users/`

## Related Domain Entities
- `User`
- `Member`
- `Organization`
- `Event`
- `UserStatus` enum
- `ProfileVisibility` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/users/me` | JWT | None | `ApiResponse<UserProfileDto>` |
| PUT | `/api/users/me` | JWT | `UpdateUserProfileRequest` | `ApiResponse<UserProfileDto>` |
| PUT | `/api/users/me/change-password` | JWT | `ChangePasswordRequest` | `ApiResponse<bool>` |
| GET | `/api/users/me/organizations` | JWT | None | `ApiResponse<ListResponse<MyOrganizationDto>>` |
| GET | `/api/users/me/events` | JWT | None | `ApiResponse<ListResponse<MyEventDto>>` |
| GET | `/api/users/me/discover/organizations` | JWT | None | `ApiResponse<ListResponse<DiscoverOrganizationDto>>` |

## Future Request DTO Names
- `UpdateUserProfileRequest`
- `ChangePasswordRequest`

## Future Response DTO Names
- `UserProfileDto`
- `MyOrganizationDto`
- `MyEventDto`
- `DiscoverOrganizationDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/userService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/userAdapter.js`

## Future Page/Component Files
- `frontend/org-frontend/src/pages/user/UserProfilePage.jsx`
- `frontend/org-frontend/src/pages/user/UserSettingsPage.jsx`
- `frontend/org-frontend/src/pages/user/UserOrganizationsPage.jsx`
- `frontend/org-frontend/src/pages/user/UserEventsPage.jsx`
- `frontend/org-frontend/src/pages/user/UserDiscoverPage.jsx`

## Required Permissions
- All routes require JWT token (authenticated user)
- No organization-specific permissions required

## Contract Notes

### UserProfileDto
- **Fields**: `Id`, `FullName`, `Email`, `PhoneNumber?`, `Dob?`, `Gender?`, `Address?`, `AvatarUrl?`, `Bio?`, `SocialLinks?`, `Status`, `ProfileVisibility?`, `LastLoginAtUtc?`, `CreatedAtUtc`
- **Note**: Full user profile with all optional fields
- **Security**: No `PasswordHash` exposed

### UpdateUserProfileRequest
- **Fields**: `FullName`, `PhoneNumber?`, `Dob?`, `Gender?`, `Address?`, `AvatarUrl?`, `Bio?`, `SocialLinks?`, `ProfileVisibility?`
- **Note**: User can update their own profile
- **Security**: Cannot update `Email`, `Status`, `PasswordHash`

### ChangePasswordRequest
- **Fields**: `CurrentPassword`, `NewPassword`, `ConfirmNewPassword?`
- **Validation**: Current password must match, new password strength rules
- **Security**: Password is never returned in response

### MyOrganizationDto
- **Fields**: `Id`, `OrgName`, `Description?`, `AvatarUrl?`, `Status`, `MyMemberStatus`, `MyRoleName?`, `JoinedAtUtc`
- **Note**: Organizations where current user is a member
- **Important**: This belongs to **userService**, NOT organizationService

### MyEventDto
- **Fields**: `Id`, `EventName`, `Description?`, `StartDate`, `EndDate`, `Status`, `Visibility`, `OrganizationId`, `OrganizationName`
- **Note**: Events where current user is involved (as member, event member, or attendee)

### DiscoverOrganizationDto
- **Fields**: `Id`, `OrgName`, `Description?`, `AvatarUrl?`, `TotalMembers`, `Status`
- **Note**: Public organizations for discovery
- **Important**: No mock fallback, public/discover data only

## Validation Notes
- **FullName**: Required, max 100 characters
- **Email**: Cannot be changed via profile update (separate endpoint if needed)
- **PhoneNumber**: Optional, phone format validation
- **Dob**: Optional, must be in the past
- **ProfileVisibility**: Optional, default "Public"
- **CurrentPassword**: Required for password change, must match existing password
- **NewPassword**: Required, min 8 characters, strength rules

## Mapping Notes
- **Entity → DTO**: Map `User` entity to `UserProfileDto`, exclude `PasswordHash`
- **DTO → Entity**: Map `UpdateUserProfileRequest` to `User` entity, exclude `Email`, `Status`
- **Security**: Never expose `PasswordHash` in any DTO

## What is NOT Implemented in This Phase
- ❌ No real profile update logic
- ❌ No real password change logic
- ❌ No real organization/event query logic
- ❌ Only contract skeleton/TODO files

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Users/`
- **Shared Contract**: `backend/Org.Shared/Features/Users/UserContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/userService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/userAdapter.js`
- **Frontend Pages**: `UserProfilePage.jsx`, `UserSettingsPage.jsx`, `UserOrganizationsPage.jsx`, `UserEventsPage.jsx`, `UserDiscoverPage.jsx`

## Important Note
**getMyOrganizations belongs to userService**, NOT organizationService. This is a user-scoped query, not an organization-scoped query.

---

**End of Users README.md**
