# Users Module

## Module Purpose
User profile management, settings, and user-specific data retrieval.

## Scope Status
**CORE** - Full backend skeleton required

## Related Domain Entities
- `User` (Domain/Entities/User.cs)
- `Member` (Domain/Entities/Member.cs)
- `Organization` (Domain/Entities/Organization.cs)
- `Event` (Domain/Entities/Event.cs)
- Enums: `UserStatus`, `ProfileVisibility`

## Expected Backend Routes

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/users/me` | Get current user profile |
| PUT | `/api/users/me` | Update current user profile |
| PUT | `/api/users/me/change-password` | Change user password |
| GET | `/api/users/me/organizations` | Get user's organizations |
| GET | `/api/users/me/events` | Get user's events |
| GET | `/api/users/me/tasks` | Get user's assigned tasks (with event context) |
| GET | `/api/users/me/discover/organizations` | Get discoverable organizations for user |

## Required Contracts (Later - Phase 3C-3)
- `UserProfileDto` (user profile data)
- `UpdateProfileRequest` (FullName, PhoneNumber, Dob, Gender, Address, Bio, SocialLinks, ProfileVisibility)
- `ChangePasswordRequest` (CurrentPassword, NewPassword, ConfirmNewPassword)
- `UserOrganizationDto` (organization with user's membership info)
- `UserEventDto` (event with user's participation info)

## Required Permissions
- All endpoints require valid JWT token
- No specific organization permission required (user-scoped endpoints)

## Validation Rules
### Update Profile
- FullName: required, max 100 characters
- Email: cannot be changed via this endpoint
- PhoneNumber: optional, valid phone format
- Dob: optional, must be in past
- Gender: optional, valid enum value
- ProfileVisibility: optional, valid enum value

### Change Password
- CurrentPassword: required
- NewPassword: required, min 6 characters, complexity rules
- ConfirmNewPassword: must match NewPassword
- NewPassword must be different from CurrentPassword

## Mapping Rules
- `User` entity → `UserProfileDto` (exclude PasswordHash)
- `Member` + `Organization` → `UserOrganizationDto`
- `Event` + user participation → `UserEventDto`

## Error Handling Rules
- 400 Bad Request: validation errors
- 401 Unauthorized: invalid or missing JWT
- 403 Forbidden: user account suspended
- 404 Not Found: user not found
- 409 Conflict: current password incorrect (change password)
- 500 Internal Server Error: unexpected errors

## What is NOT Implemented in Phase 3C
- ❌ No real endpoint implementations
- ❌ No database queries
- ❌ No business logic
- ❌ Only TODO skeleton files

## Future Implementation Order
1. Create shared contracts in `Org.Shared/Features/Users/`
2. Implement user service layer
3. Implement GET /me endpoint
4. Implement PUT /me endpoint
5. Implement change password endpoint
6. Implement get organizations endpoint
7. Implement get events endpoint
8. Implement discover organizations endpoint
9. Add validation logic
10. Add error handling
11. Add integration tests

## Cross-layer Contract Notes
### Future Shared Contract
- `backend/Org.Shared/Features/Users/UserContracts.cs.TODO`

### Future Frontend Service
- `frontend/org-frontend/src/services/userService.js`

### Future Frontend Adapter
- `frontend/org-frontend/src/adapters/userAdapter.js`

### Future Pages/Components
- `UserProfilePage.jsx`
- `UserSettingsPage.jsx`
- `UserOrganizationsPage.jsx`
- `UserEventsPage.jsx`
- `UserDiscoverPage.jsx`

### Required Permissions
- Valid JWT token (no specific org permission)

### Status
- **CORE**

## Important Notes
- `getMyOrganizations()` belongs to **userService**, NOT organizationService
- This is a critical ownership rule to prevent confusion
- User-scoped endpoints do not require organization context
- Profile visibility controls what other users can see
