# Events Module

## Module Purpose
Event management including CRUD operations, visibility control, and public discovery.

## Scope Status
**CORE** - Full backend skeleton required

## Related Domain Entities
- `Event`, `Organization`, `Member`, `Milestone`, `EventCategory`, `OrgTask`
- Enums: `EventStatus`, `EventVisibility`

## Expected Backend Routes
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/organizations/{orgId}/events` | List organization events |
| POST | `/api/organizations/{orgId}/events` | Create event |
| GET | `/api/events/{id}` | Get event details |
| PUT | `/api/events/{id}` | Update event |
| DELETE | `/api/events/{id}` | Delete event |
| GET | `/api/events/public` | List public events |
| GET | `/api/events/{id}/public` | Get public event details |

## Required Permissions
- `org.events.view` - View events
- `org.events.create` - Create events
- `org.events.manage` - Update/delete events
- Public endpoints require no permission

## Important Notes
- EventDto should include Location, TargetParticipants, Budget, AverageRating, Tags
- EventMember and Attendee are DB foundation only, no working UI/API in base prototype
- Task path: Event → Milestone → EventCategory → Task
- AverageRating is cached value, not source of truth

## Cross-layer Contract Notes
- Future contract: `backend/Org.Shared/Features/Events/EventContracts.cs.TODO`
- Future service: `frontend/org-frontend/src/services/eventService.js`
- Future adapter: `frontend/org-frontend/src/adapters/eventAdapter.js`
- Future pages: `OrgEventsPage.jsx`, `OrgEventDetailPage.jsx`, `EventCard.jsx`
- Permissions: `org.events.view`, `org.events.create`, `org.events.manage`
- Status: **CORE**
