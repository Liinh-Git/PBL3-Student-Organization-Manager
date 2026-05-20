# Auth Endpoints

## Overview
This folder will contain FastEndpoints endpoint implementations for authentication.

## Planned Endpoints

### 1. LoginEndpoint.cs.TODO
- **Route**: `POST /api/auth/login`
- **Purpose**: Authenticate user and return JWT token
- **Request**: `LoginRequest` (Email, Password)
- **Response**: `LoginResponse` (Token, User info)
- **Permission**: Public (no auth required)

### 2. RegisterEndpoint.cs.TODO
- **Route**: `POST /api/auth/register`
- **Purpose**: Register new user account
- **Request**: `RegisterRequest` (FullName, Email, Password, ConfirmPassword)
- **Response**: `RegisterResponse` (User info, success message)
- **Permission**: Public (no auth required)

### 3. GetCurrentUserEndpoint.cs.TODO
- **Route**: `GET /api/auth/me`
- **Purpose**: Get current authenticated user information
- **Request**: None (uses JWT from Authorization header)
- **Response**: `CurrentUserResponse` (User profile data)
- **Permission**: Requires valid JWT token

## Implementation Notes
- All endpoints will use FastEndpoints base class
- JWT validation will be configured in `Infrastructure/Auth/`
- Password hashing will use BCrypt or similar
- Validation will use FluentValidation
- Error responses will follow standard `ApiResponse` format

## NOT Implemented in Phase 3C
- ❌ No real endpoint implementations
- ❌ Only `.TODO` files with structure notes
