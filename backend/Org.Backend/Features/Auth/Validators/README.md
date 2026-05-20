# Auth Validators

## Overview
This folder will contain FluentValidation validators for authentication requests.

## Planned Validators

### 1. LoginRequestValidator
**Purpose**: Validate login request

**Rules**:
- `Email`:
  - Required: "Email is required"
  - Valid email format: "Invalid email format"
  - Max length 255: "Email too long"

- `Password`:
  - Required: "Password is required"
  - Min length 6: "Password must be at least 6 characters"

### 2. RegisterRequestValidator
**Purpose**: Validate registration request

**Rules**:
- `FullName`:
  - Required: "Full name is required"
  - Max length 100: "Full name too long"
  - Min length 2: "Full name too short"

- `Email`:
  - Required: "Email is required"
  - Valid email format: "Invalid email format"
  - Max length 255: "Email too long"
  - Custom: Check uniqueness in database (async validator)

- `Password`:
  - Required: "Password is required"
  - Min length 6: "Password must be at least 6 characters"
  - Regex: At least 1 uppercase, 1 lowercase, 1 digit
  - Message: "Password must contain uppercase, lowercase, and digit"

- `ConfirmPassword`:
  - Required: "Confirm password is required"
  - Must equal Password: "Passwords do not match"

## Implementation Notes
- Use FluentValidation library
- Validators will be auto-registered with FastEndpoints
- Async validators for database checks (email uniqueness)
- Error messages should be user-friendly
- Return 400 Bad Request with validation errors

## NOT Implemented in Phase 3C
- ❌ No real validator implementations
- ❌ Only README with structure notes
