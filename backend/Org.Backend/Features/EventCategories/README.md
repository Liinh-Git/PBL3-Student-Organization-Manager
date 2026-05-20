# EventCategories Module

## Module Purpose
Event category management within milestones for task organization.

## Scope Status
**CORE** - Full backend skeleton required

## Related Domain Entities
- `EventCategory`, `Milestone`, `Department`, `OrgTask`

## Expected Backend Routes
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/milestones/{milestoneId}/categories` | List milestone categories |
| POST | `/api/milestones/{milestoneId}/categories` | Create category |
| GET | `/api/categories/{id}` | Get category details |
| PUT | `/api/categories/{id}` | Update category |
| DELETE | `/api/categories/{id}` | Delete category |

## Required Permissions
- `org.events.view` - View categories
- `org.events.manage` - CRUD categories

## Important Notes
- CategoryDto may include tasks[] array (optional)
- If tasks[] absent, frontend initializes tasks: []
- Do NOT invent separate list-by-category task endpoint
- OrderIndex should be maintained for rendering

## Cross-layer Contract Notes
- Future contract: `backend/Org.Shared/Features/EventCategories/CategoryContracts.cs.TODO`
- Future service: `frontend/org-frontend/src/services/categoryService.js`
- Future adapter: `frontend/org-frontend/src/adapters/categoryAdapter.js`
- Future component: `CategoryPanel.jsx` (inside EventDetail)
- Permissions: `org.events.view`, `org.events.manage`
- Status: **CORE**
