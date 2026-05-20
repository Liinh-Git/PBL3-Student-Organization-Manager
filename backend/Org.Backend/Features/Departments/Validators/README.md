# Departments Validators

## CreateDepartmentRequestValidator
- DeptName: required, max 200 chars
- Code: optional, max 50 chars, unique within org (service-level)
- Function: optional, max 500 chars
- ManagerId: optional, must be valid member

## UpdateDepartmentRequestValidator
- Same rules as Create

## NOT Implemented in Phase 3C
- ❌ No real validator implementations
