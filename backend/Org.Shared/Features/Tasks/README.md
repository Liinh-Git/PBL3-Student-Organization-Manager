# Tasks Module Contracts

## Module Purpose
Task management within categories (CORE inside EventDetail tree).

## Scope Status
**CORE** - Full contract skeleton required (inside EventDetail tree)

## Related Backend Feature Module
`backend/Org.Backend/Features/Tasks/`

## Related Domain Entities
- `OrgTask`, `EventCategory`, `Member`, `Department`, `TaskStatus` enum, `TaskPriority` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| POST | `/api/categories/{categoryId}/tasks` | org.events.manage | `CreateTaskRequest` | `ApiResponse<TaskDto>` |
| GET | `/api/tasks/{taskId}` | org.workspace.access | None | `ApiResponse<TaskDto>` |
| PUT | `/api/tasks/{taskId}` | org.events.manage | `UpdateTaskRequest` | `ApiResponse<TaskDto>` |
| DELETE | `/api/tasks/{taskId}` | org.events.manage | None | `ApiResponse<bool>` |
| PUT | `/api/tasks/{taskId}/status` | org.events.manage | `UpdateTaskStatusRequest` | `ApiResponse<TaskDto>` |
| PUT | `/api/tasks/{taskId}/assign` | org.events.manage | `AssignTaskRequest` | `ApiResponse<TaskDto>` |

## Future Request DTO Names
- `CreateTaskRequest`, `UpdateTaskRequest`, `UpdateTaskStatusRequest`, `AssignTaskRequest`

## Future Response DTO Names
- `TaskDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/taskService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/taskAdapter.js`

## Future Page/Component Files
- `frontend/org-frontend/src/components/event-detail/TaskCard.jsx` (inside EventDetail)

## Required Permissions
- **Get**: org.workspace.access
- **Create/Update/Delete/Status/Assign**: org.events.manage

## Contract Notes

### TaskDto
- **Fields**: `Id`, `EventCategoryId`, `TaskName`, `Description?`, `AssigneeId?`, `AssigneeName?`, `DeptId?`, `DepartmentName?`, `Priority`, `Status`, `Deadline?`, `Note?`, `CreatedByMemberId?`, `CompletedAt?`, `CreatedAtUtc`, `UpdatedAtUtc`
- **Note**: Task belongs to EventCategory
- **Important**: Single assignee only (no multi-assignee in v1)

### CreateTaskRequest
- **Fields**: `TaskName`, `Description?`, `AssigneeId?`, `DeptId?`, `Priority`, `Deadline?`, `Note?`
- **Validation**: TaskName required
- **Important**: Create task response should return TaskDto so frontend can append locally

### UpdateTaskRequest
- **Fields**: Same as CreateTaskRequest plus `Status`

### UpdateTaskStatusRequest
- **Fields**: `Status`
- **Note**: Quick status update endpoint

### AssignTaskRequest
- **Fields**: `AssigneeId?`
- **Note**: Quick assign endpoint

## Validation Notes
- **TaskName**: Required, max 200 characters
- **Priority**: Required, Low/Medium/High/Urgent
- **Status**: Required, Todo/InProgress/Done
- **AssigneeId**: Optional, must be a Member in same organization
- **DeptId**: Optional, must belong to same organization

## Mapping Notes
- **Entity → DTO**: Map `OrgTask` entity to `TaskDto`, include assignee/department names
- **DTO → Entity**: Map request DTOs to `OrgTask` entity

## What is NOT Implemented in This Phase
- ❌ No real CRUD logic
- ❌ No /org/tasks aggregate board (PROTOTYPE_ONLY placeholder only)
- ❌ Only contract skeleton/TODO files

## Critical Note
**Task is CORE inside EventDetail tree**. Only `/org/tasks` aggregate board is PROTOTYPE_ONLY. Create task response should return TaskDto so frontend can append locally.

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Tasks/`
- **Shared Contract**: `backend/Org.Shared/Features/Tasks/TaskContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/taskService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/taskAdapter.js`
- **Frontend Components**: `TaskCard.jsx` (inside EventDetail)

---

**End of Tasks README.md**
