# Discover Module

## Module Purpose
Public discovery of organizations and events.

## Scope Status
**SUPPORTING** - Full backend skeleton required

## Related Domain Entities
- `Organization`, `Event`
- Enums: `OrgStatus`, `EventStatus`, `EventVisibility`

## Expected Backend Routes
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/discover/organizations` | Discover public organizations |
| GET | `/api/discover/events` | Discover public events |

## Required Permissions
- Valid JWT token (authenticated users can discover)

## Important Notes
- Supporting module for discovery features
- Can aggregate public organizations/events
- No mock fallback
- Filter by visibility and status

## Cross-layer Contract Notes
- Future contract: `backend/Org.Shared/Features/Discover/DiscoverContracts.cs.TODO`
- Future service: `frontend/org-frontend/src/services/discoverService.js`
- Future adapter: `frontend/org-frontend/src/adapters/discoverAdapter.js`
- Future page: `UserDiscoverPage.jsx`
- Permissions: Valid JWT token
- Status: **SUPPORTING**
