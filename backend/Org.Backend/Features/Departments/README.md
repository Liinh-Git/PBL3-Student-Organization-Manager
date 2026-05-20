# Departments Module

## Module Purpose
Department management within organizations including CRUD operations and manager assignment.

## Scope Status
**CORE** - Full backend skeleton required

## Related Domain Entities
- `Department`, `Organization`, `Member`
- Enums: `DepartmentStatus`

## Expected Backend Routes
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/organizations/{orgId}/departments` | List departments |
| POST | `/api/organizations/{orgId}/departments` | Create department |
| GET | `/api/departments/{id}` | Get department details |
| PUT | `/api/departments/{id}` | Update department |
| DELETE | `/api/departments/{id}` | Delete department |

## Required Permissions
- `org.departments.view` - View departments
- `org.departments.manage` - CRUD departments

## Important Notes
- ManagerId points to Member, not User
- Department.Code uniqueness is service-level check
- Manager must be member of same organization

## Cross-layer Contract Notes
- Future contract: `backend/Org.Shared/Features/Departments/DepartmentContracts.cs.TODO`
- Future service: `frontend/org-frontend/src/services/departmentService.js`
- Future adapter: `frontend/org-frontend/src/adapters/departmentAdapter.js`
- Future page: `OrgDepartmentsPage.jsx`
- Permissions: `org.departments.view`, `org.departments.manage`
- Status: **CORE**
