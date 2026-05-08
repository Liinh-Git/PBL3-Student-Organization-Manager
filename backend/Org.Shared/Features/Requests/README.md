# Requests Module Contracts

## Module Purpose
Request join organization workflow (submit, review, approve/reject).

## Scope Status
**CORE** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/Requests/`

## Related Domain Entities
- `Request`, `User`, `Organization`, `Department`, `Member`, `RequestType` enum, `RequestStatus` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/organizations/{orgId}/requests` | org.requests.view | None | `ApiResponse<ListResponse<RequestDto>>` |
| POST | `/api/organizations/{orgId}/requests` | JWT | `CreateRequestRequest` | `ApiResponse<RequestDto>` |
| GET | `/api/requests/{requestId}` | org.requests.view | None | `ApiResponse<RequestDto>` |
| POST | `/api/organizations/requests/{requestId}/review` | org.requests.review/approve | `ReviewRequestRequest` | `ApiResponse<RequestDto>` |

## Future Request DTO Names
- `CreateRequestRequest`, `ReviewRequestRequest`

## Future Response DTO Names
- `RequestDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/requestService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/requestAdapter.js`

## Future Page/Component Files
- `frontend/org-frontend/src/pages/org/OrgRequestsPage.jsx`

## Required Permissions
- **List/Get**: org.requests.view
- **Create**: JWT (any authenticated user)
- **Review**: org.requests.review / org.requests.approve

## Contract Notes

### RequestDto
- **Fields**: `Id`, `SenderId`, `SenderName`, `OrganizationId`, `RequestType`, `Title?`, `Content`, `DesiredDepartmentId?`, `DesiredDepartmentName?`, `DesiredPosition?`, `Status`, `ReviewNote?`, `ReviewedByMemberId?`, `ReviewedByMemberName?`, `ReviewedAt?`, `CreatedAtUtc`
- **Note**: Supports join organization workflow

### CreateRequestRequest
- **Fields**: `RequestType`, `Title?`, `Content`, `DesiredDepartmentId?`, `DesiredPosition?`
- **Validation**: Content required, RequestType required

### ReviewRequestRequest
- **Fields**: `Status`, `ReviewNote?`
- **Validation**: Status must be Approved or Rejected
- **Note**: ReviewedByMemberId is set from JWT token

## Validation Notes
- **RequestType**: Required, JoinOrganization/DepartmentChange/RoleChange/EventParticipation/Other
- **Content**: Required, max 1000 characters
- **Status**: Required for review, Approved/Rejected

## Mapping Notes
- **Entity → DTO**: Map `Request` entity to `RequestDto`, include sender/department/reviewer names
- **DTO → Entity**: Map request DTOs to `Request` entity

## What is NOT Implemented in This Phase
- ❌ No real request workflow logic
- ❌ Only contract skeleton/TODO files

## Important Note
**This is the foundation for join organization workflow**. Other request types can be added later when DTO/API is confirmed.

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Requests/`
- **Shared Contract**: `backend/Org.Shared/Features/Requests/RequestContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/requestService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/requestAdapter.js`
- **Frontend Pages**: `OrgRequestsPage.jsx`

---

**End of Requests README.md**
