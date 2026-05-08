# Members Module

## Module Purpose
Organization membership management including member CRUD, department assignment, and role assignment.

## Scope Status
**CORE** - Full backend skeleton required

## Related Domain Entities
- `Member`, `User`, `Organization`, `Department`, `Role`
- Enums: `MemberStatus`

## Expected Backend Routes
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/organizations/{orgId}/members` | List organization members |
| POST | `/api/organizations/{orgId}/members` | Add member to organization |
| PUT | `/api/members/{id}/department` | Update member department |
| DELETE | `/api/members/{id}` | Remove member from organization |

## Required Permissions
- `org.members.view` - View members
- `org.members.manage` - Add/remove members
- `org.workspace.access` - Access organization

## Important Notes
- Role assignment belongs to RolesPermissions module, NOT here
- Do not duplicate role update logic in Members module
- Member.RoleId is canonical source of truth
- MemberRole enum is for hierarchy/default mapping only

## Cross-layer Contract Notes
- Future contract: `backend/Org.Shared/Features/Members/MemberContracts.cs.TODO`
- Future service: `frontend/org-frontend/src/services/memberService.js`
- Future adapter: `frontend/org-frontend/src/adapters/memberAdapter.js`
- Future page: `OrgMembersPage.jsx`
- Permissions: `org.members.view`, `org.members.manage`
- Status: **CORE**
