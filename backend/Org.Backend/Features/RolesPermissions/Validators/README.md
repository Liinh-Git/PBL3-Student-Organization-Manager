# RolesPermissions Validators

## CreateRoleRequestValidator
- RoleName: required, max 100 chars, unique within org
- Description: optional, max 500 chars
- PermissionIds: required, must be valid permission IDs

## UpdateRoleRequestValidator
- Same rules as Create

## AssignRoleRequestValidator
- RoleId: required, must be valid role in same organization

## NOT Implemented in Phase 3C
- ❌ No real validator implementations
