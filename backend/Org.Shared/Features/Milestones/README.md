# Milestones Module Contracts

## Module Purpose
Milestone management within events (inside EventDetail tree).

## Scope Status
**CORE** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/Milestones/`

## Related Domain Entities
- `Milestone`, `Event`, `EventCategory`, `MilestoneStatus` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/events/{eventId}/milestones` | org.workspace.access | None | `ApiResponse<ListResponse<MilestoneDto>>` |
| POST | `/api/events/{eventId}/milestones` | org.events.manage | `CreateMilestoneRequest` | `ApiResponse<MilestoneDto>` |
| GET | `/api/milestones/{id}` | org.workspace.access | None | `ApiResponse<MilestoneDto>` |
| PUT | `/api/milestones/{id}` | org.events.manage | `UpdateMilestoneRequest` | `ApiResponse<MilestoneDto>` |
| DELETE | `/api/milestones/{id}` | org.events.manage | None | `ApiResponse<bool>` |

## Future Request DTO Names
- `CreateMilestoneRequest`, `UpdateMilestoneRequest`

## Future Response DTO Names
- `MilestoneDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/milestoneService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/milestoneAdapter.js`

## Future Page/Component Files
- `frontend/org-frontend/src/components/event-detail/MilestonePanel.jsx` (inside EventDetail)

## Required Permissions
- **List/Get**: org.workspace.access
- **Create/Update/Delete**: org.events.manage

## Contract Notes

### MilestoneDto
- **Fields**: `Id`, `EventId`, `Title`, `Description?`, `OrderIndex`, `StartDate?`, `EndDate?`, `Status`, `CreatedAtUtc`
- **Note**: OrderIndex maintained for timeline rendering

### CreateMilestoneRequest
- **Fields**: `Title`, `Description?`, `OrderIndex`, `StartDate?`, `EndDate?`
- **Validation**: Title required

### UpdateMilestoneRequest
- **Fields**: Same as CreateMilestoneRequest plus `Status`

## Validation Notes
- **Title**: Required, max 200 characters
- **OrderIndex**: Required, used for timeline ordering
- **StartDate/EndDate**: Optional

## Mapping Notes
- **Entity → DTO**: Map `Milestone` entity to `MilestoneDto`
- **DTO → Entity**: Map request DTOs to `Milestone` entity

## What is NOT Implemented in This Phase
- ❌ No real CRUD logic
- ❌ Only contract skeleton/TODO files

## Important Note
**Milestones are inside EventDetail tree**. OrderIndex should be kept stable for timeline rendering.

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Milestones/`
- **Shared Contract**: `backend/Org.Shared/Features/Milestones/MilestoneContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/milestoneService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/milestoneAdapter.js`
- **Frontend Components**: `MilestonePanel.jsx` (inside EventDetail)

---

**End of Milestones README.md**
