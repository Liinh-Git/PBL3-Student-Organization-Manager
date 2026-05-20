# Events Module Contracts

## Module Purpose
Event CRUD, visibility control, and public event discovery.

## Scope Status
**CORE** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/Events/`

## Related Domain Entities
- `Event`, `Organization`, `Member`, `Milestone`, `EventCategory`, `OrgTask`, `EventStatus` enum, `EventVisibility` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/organizations/{orgId}/events` | org.workspace.access | None | `ApiResponse<ListResponse<EventDto>>` |
| POST | `/api/organizations/{orgId}/events` | org.events.create | `CreateEventRequest` | `ApiResponse<EventDto>` |
| GET | `/api/events/{id}` | org.workspace.access | None | `ApiResponse<EventDto>` |
| PUT | `/api/events/{id}` | org.events.manage | `UpdateEventRequest` | `ApiResponse<EventDto>` |
| DELETE | `/api/events/{id}` | org.events.manage | None | `ApiResponse<bool>` |
| GET | `/api/events/public` | Public | None | `ApiResponse<ListResponse<EventPublicDto>>` |
| GET | `/api/events/{id}/public` | Public | None | `ApiResponse<EventPublicDto>` |

## Future Request DTO Names
- `CreateEventRequest`, `UpdateEventRequest`

## Future Response DTO Names
- `EventDto`, `EventSummaryDto`, `EventPublicDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/eventService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/eventAdapter.js`

## Future Page/Component Files
- `frontend/org-frontend/src/pages/org/OrgEventsPage.jsx`
- `frontend/org-frontend/src/pages/org/OrgEventDetailPage.jsx`
- `frontend/org-frontend/src/components/event/EventCard.jsx`

## Required Permissions
- **List/Get inside org**: org.workspace.access
- **Create**: org.events.create
- **Update/Delete**: org.events.manage
- **Public events**: Public (no permission)

## Contract Notes

### EventDto
- **Fields**: `Id`, `OrganizationId`, `EventName`, `Description?`, `StartDate`, `EndDate`, `Status`, `Visibility`, `Location?`, `TargetParticipants?`, `Budget?`, `AverageRating?`, `Tags?`, `CreatedAtUtc`, `UpdatedAtUtc`
- **Note**: Full event details
- **Important**: Optional fields must remain nullable, do not fake values

### EventSummaryDto
- **Fields**: `Id`, `EventName`, `Description?`, `StartDate`, `EndDate`, `Status`, `Visibility`
- **Note**: Lightweight version for list views

### EventPublicDto
- **Fields**: `Id`, `EventName`, `Description?`, `StartDate`, `EndDate`, `Status`, `Location?`, `OrganizationId`, `OrganizationName`
- **Note**: Public-facing version

### CreateEventRequest
- **Fields**: `EventName`, `Description?`, `StartDate`, `EndDate`, `Visibility`, `Location?`, `TargetParticipants?`, `Budget?`, `Tags?`
- **Validation**: EventName required, StartDate < EndDate

### UpdateEventRequest
- **Fields**: Same as CreateEventRequest plus `Status`

## Validation Notes
- **EventName**: Required, max 200 characters
- **StartDate/EndDate**: Required, StartDate must be before EndDate
- **Visibility**: Required, Public/OrganizationOnly/Private
- **Budget**: Optional, decimal nullable
- **TargetParticipants**: Optional, int nullable

## Mapping Notes
- **Entity → DTO**: Map `Event` entity to DTOs
- **DTO → Entity**: Map request DTOs to `Event` entity
- **Important**: Do not fake TargetParticipants/Budget/AverageRating if missing

## What is NOT Implemented in This Phase
- ❌ No real CRUD logic
- ❌ No EventMember/Attendee UI (DB foundation only)
- ❌ Only contract skeleton/TODO files

## Important Note
**EventMember/Attendee UI is not required in base prototype**. Task path goes through Milestone → EventCategory → Task.

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Events/`
- **Shared Contract**: `backend/Org.Shared/Features/Events/EventContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/eventService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/eventAdapter.js`
- **Frontend Pages**: `OrgEventsPage.jsx`, `OrgEventDetailPage.jsx`, `EventCard.jsx`

---

**End of Events README.md**
