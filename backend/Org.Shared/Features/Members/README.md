# Members Module Contracts

## Module Purpose
Organization membership management (list, add, remove, department assignment).

## Scope Status
**CORE** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/Members/`

## Related Domain Entities
- `Member`, `User`, `Organization`, `Department`, `Role`, `MemberStatus` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/organizations/{orgId}/members` | org.workspace.access | None | `ApiResponse<ListResponse<MemberDto>>` |
| POST | `/api/organizations/{orgId}/members` | org.members.manage | `AddMemberRequest` | `ApiResponse<MemberDto>` |
| PUT | `/api/members/{id}/department` | org.members.manage | `UpdateMemberDepartmentRequest` | `ApiResponse<MemberDto>` |
| DELETE | `/api/members/{id}` | org.members.manage | None | `ApiResponse<bool>` |

## Future Request DTO Names
- `AddMemberRequest`
- `UpdateMemberDepartmentRequest`

## Future Response DTO Names
- `MemberDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/memberService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/memberAdapter.js`

## Future Page/Component Files
- `frontend/org-frontend/src/pages/org/OrgMembersPage.jsx`

## Required Permissions
- **List members**: org.workspace.access
- **Add member**: org.members.manage
- **Update department**: org.members.manage
- **Remove member**: org.members.manage

## Contract Notes

### MemberDto
- **Fields**: `Id`, `OrganizationId`, `UserId`, `DepartmentId?`, `RoleId?`, `StudentCode?`, `FullName`, `Email`, `RoleName?`, `DepartmentName?`, `Status`, `JoinedAtUtc`
- **Note**: Includes user info for display
- **Important**: RoleId is canonical, role assignment belongs to RolesPermissions module

### AddMemberRequest
- **Fields**: `UserId`, `DepartmentId?`, `StudentCode?`
- **Note**: Add existing user to organization
- **Validation**: User must exist, not already a member

### UpdateMemberDepartmentRequest
- **Fields**: `DepartmentId?`
- **Note**: Update member's department assignment
- **Validation**: Department must belong to same organization

## Validation Notes
- **UserId**: Required, must exist
- **DepartmentId**: Optional, must belong to same organization
- **StudentCode**: Optional, max 50 characters

## Mapping Notes
- **Entity → DTO**: Map `Member` entity to `MemberDto`, include user/role/department names
- **DTO → Entity**: Map request DTOs to `Member` entity

## What is NOT Implemented in This Phase
- ❌ No real member CRUD logic
- ❌ Only contract skeleton/TODO files

## Important Note
**Role assignment is NOT here**. Role assignment belongs to RolesPermissions module. RoleId is canonical.

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Members/`
- **Shared Contract**: `backend/Org.Shared/Features/Members/MemberContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/memberService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/memberAdapter.js`
- **Frontend Pages**: `OrgMembersPage.jsx`

---

**End of Members README.md**
