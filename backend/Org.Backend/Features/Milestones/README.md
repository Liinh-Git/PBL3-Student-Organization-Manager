# Milestones Module

## Module Purpose
Milestone management within events for planning and organization.

## Scope Status
**CORE** - Full backend skeleton required

## Related Domain Entities
- `Milestone`, `Event`, `EventCategory`
- Enums: `MilestoneStatus`

## Expected Backend Routes
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/events/{eventId}/milestones` | List event milestones |
| POST | `/api/events/{eventId}/milestones` | Create milestone |
| GET | `/api/milestones/{id}` | Get milestone details |
| PUT | `/api/milestones/{id}` | Update milestone |
| DELETE | `/api/milestones/{id}` | Delete milestone |

## Required Permissions
- `org.events.view` - View milestones
- `org.events.manage` - CRUD milestones

## Important Notes
- Milestones belong to Events
- OrderIndex should be maintained for timeline rendering
- Categories belong to Milestones

## Cross-layer Contract Notes
- Future contract: `backend/Org.Shared/Features/Milestones/MilestoneContracts.cs.TODO`
- Future service: `frontend/org-frontend/src/services/milestoneService.js`
- Future adapter: `frontend/org-frontend/src/adapters/milestoneAdapter.js`
- Future component: `MilestonePanel.jsx` (inside EventDetail)
- Permissions: `org.events.view`, `org.events.manage`
- Status: **CORE**
