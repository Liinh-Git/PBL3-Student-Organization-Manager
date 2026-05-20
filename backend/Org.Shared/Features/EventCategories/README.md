# EventCategories Module Contracts

## Module Purpose
Category management within milestones (inside EventDetail tree).

## Scope Status
**CORE** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/EventCategories/`

## Related Domain Entities
- `EventCategory`, `Milestone`, `Department`, `OrgTask`

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/milestones/{milestoneId}/categories` | org.workspace.access | None | `ApiResponse<ListResponse<EventCategoryDto>>` |
| POST | `/api/milestones/{milestoneId}/categories` | org.events.manage | `CreateEventCategoryRequest` | `ApiResponse<EventCategoryDto>` |
| GET | `/api/categories/{id}` | org.workspace.access | None | `ApiResponse<EventCategoryDto>` |
| PUT | `/api/categories/{id}` | org.events.manage | `UpdateEventCategoryRequest` | `ApiResponse<EventCategoryDto>` |
| DELETE | `/api/categories/{id}` | org.events.manage | None | `ApiResponse<bool>` |

## Future Request DTO Names
- `CreateEventCategoryRequest`, `UpdateEventCategoryRequest`

## Future Response DTO Names
- `EventCategoryDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/categoryService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/categoryAdapter.js`

## Future Page/Component Files
- `frontend/org-frontend/src/components/event-detail/CategoryPanel.jsx` (inside EventDetail)

## Required Permissions
- **List/Get**: org.workspace.access
- **Create/Update/Delete**: org.events.manage

## Contract Notes

### EventCategoryDto
- **Fields**: `Id`, `MilestoneId`, `CategoryName`, `Description?`, `OrderIndex`, `OwnerDepartmentId?`, `OwnerDepartmentName?`, `Tasks?`
- **Note**: `Tasks` array is optional
- **Critical**: If `Tasks` absent, frontend initializes `tasks: []`
- **Important**: Do NOT invent a list-by-category task endpoint

### CreateEventCategoryRequest
- **Fields**: `CategoryName`, `Description?`, `OrderIndex`, `OwnerDepartmentId?`
- **Validation**: CategoryName required

### UpdateEventCategoryRequest
- **Fields**: Same as CreateEventCategoryRequest

## Validation Notes
- **CategoryName**: Required, max 200 characters
- **OrderIndex**: Required, used for ordering
- **OwnerDepartmentId**: Optional, must belong to same organization

## Mapping Notes
- **Entity → DTO**: Map `EventCategory` entity to `EventCategoryDto`
- **DTO → Entity**: Map request DTOs to `EventCategory` entity
- **Tasks handling**: CategoryDto may include `tasks[]` array (optional)

## What is NOT Implemented in This Phase
- ❌ No real CRUD logic
- ❌ Only contract skeleton/TODO files

## Critical Note
**CategoryDto may include tasks[] array (optional)**. If absent, frontend initializes `tasks: []`. Do NOT invent separate list-by-category task endpoint.

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/EventCategories/`
- **Shared Contract**: `backend/Org.Shared/Features/EventCategories/CategoryContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/categoryService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/categoryAdapter.js`
- **Frontend Components**: `CategoryPanel.jsx` (inside EventDetail)

---

**End of EventCategories README.md**
