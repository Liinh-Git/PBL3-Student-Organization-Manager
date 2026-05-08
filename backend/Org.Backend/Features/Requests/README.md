# Requests Module

## Module Purpose
Request management for join organization workflow and review process.

## Scope Status
**CORE** - Full backend skeleton required

## Related Domain Entities
- `Request`, `User`, `Organization`, `Department`, `Member`
- Enums: `RequestType`, `RequestStatus`

## Expected Backend Routes
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/organizations/{orgId}/requests` | List organization requests |
| POST | `/api/organizations/{orgId}/requests` | Submit request |
| GET | `/api/requests/{requestId}` | Get request details |
| POST | `/api/organizations/requests/{requestId}/review` | Review request (approve/reject) |

## Required Permissions
- `org.requests.view` - View requests
- `org.requests.review` - Review requests
- `org.requests.approve` - Approve requests

## Important Notes
- Request supports join organization/review workflow
- DesiredDepartmentId is nullable
- ReviewedByMemberId is nullable
- RequestType enum includes JoinOrganization, DepartmentChange, RoleChange, EventParticipation, Other

## Cross-layer Contract Notes
- Future contract: `backend/Org.Shared/Features/Requests/RequestContracts.cs.TODO`
- Future service: `frontend/org-frontend/src/services/requestService.js`
- Future adapter: `frontend/org-frontend/src/adapters/requestAdapter.js`
- Future page: `OrgRequestsPage.jsx`
- Permissions: `org.requests.view`, `org.requests.review`, `org.requests.approve`
- Status: **CORE**
