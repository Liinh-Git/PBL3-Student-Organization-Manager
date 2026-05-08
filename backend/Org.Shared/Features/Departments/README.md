# Departments Module Contracts

## Module Purpose
Department CRUD and manager assignment within organizations.

## Scope Status
**CORE** - Full contract skeleton required

## Related Backend Feature Module
`backend/Org.Backend/Features/Departments/`

## Related Domain Entities
- `Department`, `Organization`, `Member`, `DepartmentStatus` enum

## Expected Backend Routes
| Method | Route | Permission | Request DTO | Response DTO |
|---|---|---|---|---|
| GET | `/api/organizations/{orgId}/departments` | org.workspace.access | None | `ApiResponse<ListResponse<DepartmentDto>>` |
| POST | `/api/organizations/{orgId}/departments` | org.departments.manage | `CreateDepartmentRequest` | `ApiResponse<DepartmentDto>` |
| GET | `/api/departments/{id}` | org.workspace.access | None | `ApiResponse<DepartmentDto>` |
| PUT | `/api/departments/{id}` | org.departments.manage | `UpdateDepartmentRequest` | `ApiResponse<DepartmentDto>` |
| DELETE | `/api/departments/{id}` | org.departments.manage | None | `ApiResponse<bool>` |

## Future Request DTO Names
- `CreateDepartmentRequest`, `UpdateDepartmentRequest`

## Future Response DTO Names
- `DepartmentDto`

## Future Frontend Service File
`frontend/org-frontend/src/services/departmentService.js`

## Future Adapter File
`frontend/org-frontend/src/adapters/departmentAdapter.js`

## Future Page/Component Files
- `frontend/org-frontend/src/pages/org/OrgDepartmentsPage.jsx`

## Required Permissions
- **List/Get**: org.workspace.access
- **Create/Update/Delete**: org.departments.manage

## Contract Notes

### DepartmentDto
- **Fields**: `Id`, `OrganizationId`, `DeptName`, `Code?`, `Function?`, `ManagerId?`, `ManagerName?`, `Status`, `CreatedAtUtc`
- **Note**: ManagerId points to Member, not User

### CreateDepartmentRequest
- **Fields**: `DeptName`, `Code?`, `Function?`, `ManagerId?`
- **Validation**: DeptName required, Code uniqueness is service-level

### UpdateDepartmentRequest
- **Fields**: Same as CreateDepartmentRequest

## Validation Notes
- **DeptName**: Required, max 100 characters
- **Code**: Optional, uniqueness is service-level check
- **ManagerId**: Optional, must be a Member in same organization

## Mapping Notes
- **Entity → DTO**: Map `Department` entity to `DepartmentDto`, include manager name
- **DTO → Entity**: Map request DTOs to `Department` entity

## What is NOT Implemented in This Phase
- ❌ No real CRUD logic
- ❌ Only contract skeleton/TODO files

## Important Note
**ManagerId points to Member**, not User. Department.Code uniqueness is service-level check.

## Cross-layer Notes
- **Backend Feature**: `backend/Org.Backend/Features/Departments/`
- **Shared Contract**: `backend/Org.Shared/Features/Departments/DepartmentContracts.cs.TODO`
- **Frontend Service**: `frontend/org-frontend/src/services/departmentService.js`
- **Frontend Adapter**: `frontend/org-frontend/src/adapters/departmentAdapter.js`
- **Frontend Pages**: `OrgDepartmentsPage.jsx`

---

**End of Departments README.md**
