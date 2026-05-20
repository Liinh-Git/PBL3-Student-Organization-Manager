# Auth Module Contracts

## Module Purpose
Authentication and JWT token management for user login, registration, and current user retrieval.

## Scope Status
**CORE** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/Auth/`

## Related Domain Entities
- `User`
- `UserStatus` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| POST | `/api/auth/login` | Public | `LoginRequest` | `ApiResponse<AuthTokenResponse>` |
| POST | `/api/auth/register` | Public | `RegisterRequest` | `ApiResponse<AuthTokenResponse>` |
| GET | `/api/auth/me` | JWT required | None | `ApiResponse<CurrentUserResponse>` |

## Future Request DTO Names
- `LoginRequest`
- `RegisterRequest`

## Future Response DTO Names
- `AuthUserDto`
- `AuthTokenResponse`
- `CurrentUserResponse`

## Future Frontend Service File
`frontend/org-frontend/src/services/authService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/userAdapter.js`

## Future Page/Component Files
- `frontend/org-frontend/src/pages/auth/LoginPage.jsx`
- `frontend/org-frontend/src/pages/auth/RegisterPage.jsx`

## Required Permissions
- **Login**: Public (no permission required)
- **Register**: Public (no permission required)
- **Get Current User**: JWT token required (authenticated user)

## Contract Notes

### LoginRequest
- **Fields**: `Email`, `Password`
- **Validation**: Email format, password required
- **Security**: Password is never returned in response

### RegisterRequest
- **Fields**: `FullName`, `Email`, `Password`, `ConfirmPassword?`
- **Validation**: Email format, password strength, confirm password match
- **Security**: Password is never returned in response
- **Note**: `ConfirmPassword` is optional/client-side validation only

### AuthTokenResponse
- **Fields**: `AccessToken`, `TokenType`, `ExpiresAtUtc`, `User`
- **Note**: `User` is `AuthUserDto` with basic user info
- **Security**: Token must be stored securely (httpOnly cookie or secure storage)

### AuthUserDto
- **Fields**: `Id`, `FullName`, `Email`, `Status`, `AvatarUrl?`
- **Note**: Minimal user info for auth context
- **Security**: No `PasswordHash` or sensitive fields

### CurrentUserResponse
- **Fields**: Same as `AuthUserDto`
- **Purpose**: Get current authenticated user info
- **Note**: Used to refresh user context after login

## Validation Notes
- **Email**: Must be valid email format, unique in database
- **Password**: Minimum 8 characters, at least one uppercase, one lowercase, one digit (configurable)
- **FullName**: Required, max 100 characters
- **ConfirmPassword**: Must match `Password` (client-side validation)

## Mapping Notes
- **Entity → DTO**: Map `User` entity to `AuthUserDto`, exclude `PasswordHash`
- **DTO → Entity**: Map `RegisterRequest` to `User` entity, hash password before saving
- **Security**: Never expose `PasswordHash` in any DTO

## What is NOT Implemented in This Phase
- ❌ No real JWT token generation logic
- ❌ No real password hashing logic
- ❌ No real authentication middleware
- ❌ No real authorization policies
- ❌ No real user validation logic
- ❌ Only contract skeleton/TODO files

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Auth/`
- **Shared Contract**: `backend/Org.Shared/Features/Auth/AuthContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/authService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/userAdapter.js`
- **Frontend Pages**: `LoginPage.jsx`, `RegisterPage.jsx`

---

**End of Auth README.md**
