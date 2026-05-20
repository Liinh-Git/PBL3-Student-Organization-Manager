# Organizations Module Contracts

## Module Purpose
Organization CRUD, management, and public overview.

## Scope Status
**CORE** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/Organizations/`

## Related Domain Entities
- `Organization`, `Member`, `User`, `OrgStatus` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/organizations` | JWT | None | `ApiResponse<ListResponse<OrganizationSummaryDto>>` |
| POST | `/api/organizations` | JWT | `CreateOrganizationRequest` | `ApiResponse<OrganizationDto>` |
| GET | `/api/organizations/default` | JWT | None | `ApiResponse<OrganizationDto>` |
| GET | `/api/organizations/{id}` | org.workspace.access | None | `ApiResponse<OrganizationDto>` |
| PUT | `/api/organizations/{id}` | org.overview.write | `UpdateOrganizationRequest` | `ApiResponse<OrganizationDto>` |
| GET | `/api/organizations/{id}/public-overview` | Public | None | `ApiResponse<OrganizationPublicOverviewDto>` |

## Future Request DTO Names
- `CreateOrganizationRequest`
- `UpdateOrganizationRequest`

## Future Response DTO Names
- `OrganizationDto`
- `OrganizationSummaryDto`
- `OrganizationPublicOverviewDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/organizationService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/organizationAdapter.js`

## Future Page/Component Files
- `frontend/org-frontend/src/pages/org/OrgOverviewPage.jsx`
- `frontend/org-frontend/src/components/org/OrgCard.jsx`
- `frontend/org-frontend/src/components/org/OrgSwitcher.jsx`

## Required Permissions
- **List orgs**: JWT (user's orgs)
- **Create org**: JWT
- **Get org**: org.workspace.access
- **Update org**: org.overview.write
- **Public overview**: Public (no permission)

## Contract Notes

### OrganizationDto
- **Fields**: `Id`, `OrgName`, `Description?`, `AvatarUrl?`, `CoverUrl?`, `FoundingDate?`, `Location?`, `ContactEmail?`, `ContactPhone?`, `TotalMembers`, `Status`, `CreatedAtUtc`
- **Note**: Full organization details

### OrganizationSummaryDto
- **Fields**: `Id`, `OrgName`, `Description?`, `AvatarUrl?`, `TotalMembers`, `Status`
- **Note**: Lightweight version for list views

### OrganizationPublicOverviewDto
- **Fields**: `Id`, `OrgName`, `Description?`, `AvatarUrl?`, `CoverUrl?`, `FoundingDate?`, `Location?`, `TotalMembers`, `Status`
- **Note**: Public-facing version, no contact info
- **Important**: Must remain renderable even if permissions/me returns 403

### CreateOrganizationRequest
- **Fields**: `OrgName`, `Description?`, `AvatarUrl?`, `CoverUrl?`, `FoundingDate?`, `Location?`, `ContactEmail?`, `ContactPhone?`
- **Validation**: OrgName required, unique (service-level check)

### UpdateOrganizationRequest
- **Fields**: Same as CreateOrganizationRequest
- **Validation**: OrgName uniqueness (service-level check)

## Validation Notes
- **OrgName**: Required, max 100 characters, uniqueness is service-level check (not DB hard constraint)
- **ContactEmail**: Optional, email format validation
- **ContactPhone**: Optional, phone format validation

## Mapping Notes
- **Entity → DTO**: Map `Organization` entity to DTOs
- **DTO → Entity**: Map request DTOs to `Organization` entity
- **OrgName uniqueness**: Service-level check, not DB constraint

## What is NOT Implemented in This Phase
- ❌ No real CRUD logic
- ❌ No real uniqueness check
- ❌ Only contract skeleton/TODO files

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Organizations/`
- **Shared Contract**: `backend/Org.Shared/Features/Organizations/OrganizationContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/organizationService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/organizationAdapter.js`
- **Frontend Pages**: `OrgOverviewPage.jsx`, `OrgCard.jsx`, `OrgSwitcher.jsx`

---

**End of Organizations README.md**
