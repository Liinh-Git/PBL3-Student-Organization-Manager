# Discover Module Contracts

## Module Purpose
Public discovery of organizations and events.

## Scope Status
**SUPPORTING** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/Discover/`

## Related Domain Entities
- `Organization`, `Event`, `OrgStatus` enum, `EventStatus` enum, `EventVisibility` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/discover/organizations` | JWT | None | `ApiResponse<ListResponse<DiscoverOrganizationDto>>` |
| GET | `/api/discover/events` | JWT | None | `ApiResponse<ListResponse<DiscoverEventDto>>` |

## Future Request DTO Names
- None (all queries use route parameters)

## Future Response DTO Names
- `DiscoverOrganizationDto`, `DiscoverEventDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/discoverService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/discoverAdapter.js` (if needed)

## Future Page/Component Files
- `frontend/org-frontend/src/pages/user/UserDiscoverPage.jsx`

## Required Permissions
- All routes require JWT token (authenticated user)

## Contract Notes

### DiscoverOrganizationDto
- **Fields**: `Id`, `OrgName`, `Description?`, `AvatarUrl?`, `TotalMembers`, `Status`
- **Note**: Public organizations for discovery
- **Important**: No mock fallback, public/discover data only

### DiscoverEventDto
- **Fields**: `Id`, `EventName`, `Description?`, `StartDate`, `EndDate`, `Status`, `Visibility`, `OrganizationId`, `OrganizationName`, `Location?`
- **Note**: Public events for discovery
- **Important**: No mock fallback, public/discover data only

## Validation Notes
- No request DTOs, all queries use route parameters

## Mapping Notes
- **Entity → DTO**: Map `Organization`/`Event` entities to DTOs
- **Filter**: Only public/discoverable organizations and events

## What is NOT Implemented in This Phase
- ❌ No real discovery logic
- ❌ Only contract skeleton/TODO files

## Important Note
**No mock fallback**. Public/discover data only.

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Discover/`
- **Shared Contract**: `backend/Org.Shared/Features/Discover/DiscoverContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/discoverService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/discoverAdapter.js`
- **Frontend Pages**: `UserDiscoverPage.jsx`

---

**End of Discover README.md**
