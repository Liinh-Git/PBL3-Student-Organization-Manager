# Auth Services

## Overview
This folder will contain service layer implementations for authentication business logic.

## Planned Services

### 1. IAuthService / AuthService
**Purpose**: Handle authentication business logic

**Methods**:
- `Task<LoginResponse> LoginAsync(LoginRequest request)`
  - Validate credentials
  - Generate JWT token
  - Return user info with token

- `Task<RegisterResponse> RegisterAsync(RegisterRequest request)`
  - Validate email uniqueness
  - Hash password
  - Create user account
  - Return user info

- `Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId)`
  - Get user by ID
  - Validate user status
  - Return user profile

### 2. IJwtService / JwtService
**Purpose**: Handle JWT token generation and validation

**Methods**:
- `string GenerateToken(User user)`
  - Create JWT with user claims (UserId, Email, etc.)
  - Set expiration time
  - Sign with secret key

- `ClaimsPrincipal? ValidateToken(string token)`
  - Validate token signature
  - Check expiration
  - Return claims if valid

### 3. IPasswordHashService / PasswordHashService
**Purpose**: Handle password hashing and verification

**Methods**:
- `string HashPassword(string password)`
  - Hash password using BCrypt or similar
  - Return hashed password

- `bool VerifyPassword(string password, string passwordHash)`
  - Verify password against hash
  - Return true if match

## Implementation Notes
- Services will be registered in DI container
- Use BCrypt.Net-Next for password hashing
- JWT configuration will be in appsettings.json
- Token expiration: 24 hours (configurable)
- Password complexity rules: min 6 chars, at least 1 uppercase, 1 lowercase, 1 digit

## NOT Implemented in Phase 3C
- ❌ No real service implementations
- ❌ Only README with structure notes
