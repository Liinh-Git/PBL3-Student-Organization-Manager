# Tasks Module

## Module Purpose
Task management within event categories for event planning and execution.

## Scope Status
**CORE** - Full backend skeleton required (Task is CORE inside EventDetail tree)

## Related Domain Entities
- `OrgTask`, `EventCategory`, `Member`, `Department`
- Enums: `TaskStatus`, `TaskPriority`

## Expected Backend Routes
| Method | Route | Purpose |
|---|---|---|
| POST | `/api/categories/{categoryId}/tasks` | Create task |
| GET | `/api/tasks/{taskId}` | Get task details |
| PUT | `/api/tasks/{taskId}` | Update task |
| DELETE | `/api/tasks/{taskId}` | Delete task |
| PUT | `/api/tasks/{taskId}/status` | Update task status |
| PUT | `/api/tasks/{taskId}/assign` | Assign task to member |

## Required Permissions
- `org.events.view` - View tasks
- `org.events.manage` - CRUD tasks

## Important Notes
- Task belongs to EventCategory (not directly to Event)
- Single assignee only (AssigneeId points to Member)
- DeptId points to Department
- No /org/tasks aggregate endpoint in base implementation
- Create task response should return TaskDto so frontend can append locally
- Only `/org/tasks` aggregate board is PROTOTYPE_ONLY, NOT the task module itself

## Cross-layer Contract Notes
- Future contract: `backend/Org.Shared/Features/Tasks/TaskContracts.cs.TODO`
- Future service: `frontend/org-frontend/src/services/taskService.js`
- Future adapter: `frontend/org-frontend/src/adapters/taskAdapter.js`
- Future component: `TaskCard.jsx` (inside EventDetail)
- Permissions: `org.events.view`, `org.events.manage`
- Status: **CORE**

## CRITICAL DISTINCTION
- **Task module is CORE** inside EventDetail tree
- **Only `/org/tasks` aggregate board** is PROTOTYPE_ONLY
- Do NOT confuse the two
