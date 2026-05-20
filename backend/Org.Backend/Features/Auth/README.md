# Auth Module

## Module Purpose
Authentication module handling user login, registration, and session management.

## Scope Status
**CORE** - Full backend skeleton required

## Related Domain Entities
- `User` (Domain/Entities/User.cs)
- Enums: `UserStatus`

## Expected Backend Routes

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/auth/login` | User login with email/password |
| POST | `/api/auth/register` | New user registration |
| GET | `/api/auth/me` | Get current authenticated user info |
| POST | `/api/auth/logout` | Logout (optional/client-side only) |

## Required Contracts (Later - Phase 3C-3)
- `LoginRequest` (Email, Password)
- `LoginResponse` (Token, User info)
- `RegisterRequest` (FullName, Email, Password, ConfirmPassword)
- `RegisterResponse` (User info, success message)
- `CurrentUserResponse` (User profile data)

## Required Permissions
- No permission required for login/register (public endpoints)
- `GET /api/auth/me` requires valid JWT token

## Validation Rules
### Login
- Email: required, valid email format
- Password: required, min 6 characters

### Register
- FullName: required, max 100 characters
- Email: required, valid email format, unique
- Password: required, min 6 characters, complexity rules
- ConfirmPassword: must match Password

## Mapping Rules
- `User` entity → `UserDto` (exclude PasswordHash)
- Never expose `PasswordHash` in any response
- Map `UserStatus` enum to string

## Error Handling Rules
- 400 Bad Request: validation errors
- 401 Unauthorized: invalid credentials
- 409 Conflict: email already exists (register)
- 500 Internal Server Error: unexpected errors

## What is NOT Implemented in Phase 3C
- ❌ No JWT token generation/validation logic
- ❌ No password hashing implementation
- ❌ No database queries
- ❌ No business logic
- ❌ Only TODO skeleton files

## Future Implementation Order
1. Create shared contracts in `Org.Shared/Features/Auth/`
2. Implement JWT configuration in `Infrastructure/Auth/`
3. Implement password hashing service
4. Implement login endpoint with JWT generation
5. Implement register endpoint with password hashing
6. Implement `/me` endpoint with JWT validation
7. Add validation logic
8. Add error handling
9. Add integration tests

## Cross-layer Contract Notes
### Future Shared Contract
- `backend/Org.Shared/Features/Auth/AuthContracts.cs.TODO`

### Future Frontend Service
- `frontend/org-frontend/src/services/authService.js`

### Future Frontend Adapter
- `frontend/org-frontend/src/adapters/userAdapter.js`

### Future Pages/Components
- `LoginPage.jsx`
- `RegisterPage.jsx`

### Required Permissions
- Public endpoints (no permission required)

### Status
- **CORE**

## Notes
- JWT implementation is deferred to later phase
- PasswordHash field exists in User entity but hashing logic not implemented yet
- Logout can be client-side only (clear token from storage)
- No refresh token in base prototype
