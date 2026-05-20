# Organizations Module

## Module Purpose
Organization management including CRUD operations, membership, and public discovery.

## Scope Status
**CORE** - Full backend skeleton required

## Related Domain Entities
- `Organization` (Domain/Entities/Organization.cs)
- `Member` (Domain/Entities/Member.cs)
- `User` (Domain/Entities/User.cs)
- Enums: `OrgStatus`

## Expected Backend Routes

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/organizations` | List all organizations (filtered by user access) |
| POST | `/api/organizations` | Create new organization |
| GET | `/api/organizations/default` | Get user's default/first organization |
| GET | `/api/organizations/{id}` | Get organization details |
| PUT | `/api/organizations/{id}` | Update organization |
| GET | `/api/organizations/{id}/public-overview` | Get public organization overview |

## Required Contracts (Later - Phase 3C-3)
- `OrganizationDto`
- `CreateOrganizationRequest`
- `UpdateOrganizationRequest`
- `PublicOrganizationDto`

## Required Permissions
- `org.overview.read` - View organization details
- `org.overview.write` - Update organization
- `org.workspace.access` - Access organization workspace
- Public overview endpoint requires no permission

## Validation Rules
- OrgName: required, max 200 chars, unique (service-level check)
- Description: optional, max 2000 chars
- ContactEmail: optional, valid email format
- ContactPhone: optional, valid phone format

## Mapping Rules
- `Organization` entity → `OrganizationDto`
- Include member count, event count
- Filter sensitive data for public overview

## Error Handling Rules
- 400 Bad Request: validation errors
- 401 Unauthorized: missing JWT
- 403 Forbidden: insufficient permissions
- 404 Not Found: organization not found
- 409 Conflict: organization name already exists
- 500 Internal Server Error: unexpected errors

## What is NOT Implemented in Phase 3C
- ❌ No real endpoint implementations
- ❌ No database queries
- ❌ No business logic
- ❌ Only TODO skeleton files

## Cross-layer Contract Notes
### Future Shared Contract
- `backend/Org.Shared/Features/Organizations/OrganizationContracts.cs.TODO`

### Future Frontend Service
- `frontend/org-frontend/src/services/organizationService.js`

### Future Frontend Adapter
- `frontend/org-frontend/src/adapters/organizationAdapter.js`

### Future Pages/Components
- `OrgOverviewPage.jsx`
- `OrgCard.jsx`
- `OrgSwitcher.jsx`

### Required Permissions
- `org.overview.read`
- `org.overview.write`
- `org.workspace.access`

### Status
- **CORE**

## Important Notes
- Organization.OrgName uniqueness is service-level, not DB hard constraint
- Public overview flow must not fail if permissions/me returns 403
- TotalMembers is cached count, not source of truth
- getMyOrganizations belongs to userService, NOT organizationService
