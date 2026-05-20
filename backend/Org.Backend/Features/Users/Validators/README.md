# Users Validators

## Overview
FluentValidation validators for user profile and settings requests.

## Planned Validators

### UpdateProfileRequestValidator
**Rules**:
- FullName: required, max 100 chars
- PhoneNumber: optional, valid phone format
- Dob: optional, must be in past
- Gender: optional, valid enum value
- ProfileVisibility: optional, valid enum value

### ChangePasswordRequestValidator
**Rules**:
- CurrentPassword: required
- NewPassword: required, min 6 chars, complexity rules
- ConfirmNewPassword: must match NewPassword

## NOT Implemented in Phase 3C
- ❌ No real validator implementations
- ❌ Only README with structure notes
