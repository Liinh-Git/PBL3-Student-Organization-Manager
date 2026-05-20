# Tasks Validators

## CreateTaskRequestValidator
- TaskName: required, max 200 chars
- Description: optional, max 2000 chars
- AssigneeId: optional, must be valid member
- DeptId: optional, must be valid department
- Priority: required, valid TaskPriority enum
- Deadline: optional, valid future date
- Status: required, valid TaskStatus enum

## UpdateTaskRequestValidator
- Same rules as Create

## UpdateTaskStatusRequestValidator
- Status: required, valid TaskStatus enum

## AssignTaskRequestValidator
- AssigneeId: required, must be valid member

## NOT Implemented in Phase 3C
- ❌ No real validator implementations
