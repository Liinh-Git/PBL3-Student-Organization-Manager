# PHASE_3C_REQUIREMENTS_SPEC

## 1. Purpose

Phase 3C creates the **architecture and prototype skeleton handoff** for the PBL3 Student Organization Manager rescue project. This phase produces:

- Backend feature skeleton with TODO implementation notes
- Shared contract skeleton with DTO placeholders
- Frontend React skeleton with route/page/service/adapter/component structure
- EventDetail tree component skeleton
- Cross-layer documentation and traceability
- Detailed TODO implementation comments for future development

**Phase 3C is NOT implementation.** It creates the skeleton structure and handoff documentation that will guide future implementation work.

---

## 2. Current State

### Phase 3A - COMPLETED ✅
- Repository foundation created
- .NET solution and projects configured
- React + Vite frontend skeleton created
- Package dependencies installed
- Build verification passed

### Phase 3B.1 - COMPLETED ✅
- `DOMAIN_ENTITY_LOCK_V1.md` created
- Domain model locked and documented
- 22 entities specified with relationships
- 21 enums specified
- Delete behaviors, indexes, and constraints defined

### Phase 3B.2 - COMPLETED ✅
- All 22 domain entities implemented
- All 21 domain enums implemented
- `AppDbContext` configured with DbSets
- EF Core configurations created for all entities
- Soft-delete global query filter applied
- Build verification passed (no errors)

### Phase 3B.3 - PAUSED ⏸️
- Migration creation is paused
- Database update is paused
- Seeding is paused
- No database operations will be performed in Phase 3C

---

## 3. Absolute Non-Goals

Phase 3C explicitly **DOES NOT** include:

### Database Operations
- ❌ No migration creation (`dotnet ef migrations add`)
- ❌ No database update (`dotnet ef database update`)
- ❌ No `Database.Migrate()` calls
- ❌ No database seeding
- ❌ No admin/role/permission seeding
- ❌ No connection to production database

### Backend Implementation
- ❌ No real FastEndpoints handler implementations
- ❌ No business service logic
- ❌ No actual database access code
- ❌ No validation logic implementation
- ❌ No authorization policy implementation
- ❌ No JWT token generation/validation logic

### Frontend Implementation
- ❌ No real API calls to backend
- ❌ No mock data usage
- ❌ No fake data
- ❌ No fake success responses
- ❌ No Blazor code copying
- ❌ No working business logic

### Excluded Modules
- ❌ No Posts/Comments (hard-excluded from rescue v1)
- ❌ No working Messages/Chat module
- ❌ No working Finance module
- ❌ No working Reports page beyond placeholder
- ❌ No working Resources page beyond placeholder
- ❌ No working `/org/tasks` aggregate board (PROTOTYPE_ONLY placeholder only)

---

## 4. Core System Chain

The core domain chain **MUST** be preserved throughout Phase 3C:

```
Organization → Member → Event → Milestone → EventCategory → Task
```

### Critical Rules

1. **Task is CORE** inside the EventDetail tree
2. **Only `/org/tasks` aggregate board** is PROTOTYPE_ONLY
3. **EventMember** is event staff/organizer (DB foundation only, no working UI in base prototype)
4. **Attendee** is event participant/registration/check-in (DB foundation only, no working UI in base prototype)
5. **EventMember and Attendee** are included in DB v1 to preserve domain integrity, but no working UI/API is required in base prototype

---

## 5. Module Status Classification

### CORE Modules (Full Skeleton Required)

These modules require complete backend/contract/frontend skeleton with TODO notes:

| Module | Backend Feature | Shared Contract | Frontend Service | Frontend Pages |
|---|---|---|---|---|
| Auth | ✅ | ✅ | ✅ | Login, Register |
| Users | ✅ | ✅ | ✅ | Profile, Settings, MyOrgs, MyEvents |
| Organizations | ✅ | ✅ | ✅ | Overview, CRUD |
| Members | ✅ | ✅ | ✅ | List, Add, Remove, Role Assignment |
| Departments | ✅ | ✅ | ✅ | List, CRUD, Manager, Members |
| Events | ✅ | ✅ | ✅ | List, CRUD, Detail, Visibility |
| Milestones | ✅ | ✅ | ✅ | Inside EventDetail |
| EventCategories | ✅ | ✅ | ✅ | Inside EventDetail |
| Tasks | ✅ | ✅ | ✅ | Inside EventDetail (CORE) |
| Requests | ✅ | ✅ | ✅ | Submit, Review, Approve |
| Notifications | ✅ | ✅ | ✅ | Badge, List, Mark Read |
| RolesPermissions | ✅ | ✅ | ✅ | Role CRUD, Permission Gating |

### SUPPORTING Modules (Full Skeleton Required)

| Module | Backend Feature | Shared Contract | Frontend Service | Frontend Pages |
|---|---|---|---|---|
| Friends | ✅ | ✅ | ✅ | Friends List, Friend Requests |
| Discover | ✅ | ✅ | ✅ | Discover Orgs/Events |
| User Profile/Settings | ✅ | ✅ | ✅ | Profile Update, Change Password |

### DB_FOUNDATION_ONLY (Notes Only, No Working UI)

These entities exist in DB v1 but have **no working UI/API** in base prototype:

| Entity | Treatment |
|---|---|
| EventMembers | Backend/contract notes only; no working endpoint/UI |
| Attendees | Backend/contract notes only; no working endpoint/UI |
| DigitalAssets | Backend/contract notes only; no working endpoint/UI |
| EventRatings | Backend/contract notes only; no working endpoint/UI |
| EventReports | Backend/contract notes only; no working endpoint/UI |
| Resources | Backend/contract notes only; no working endpoint/UI |
| ActivityHistory | Backend/contract notes only; no working endpoint/UI |

### PROTOTYPE_ONLY (Placeholder Pages Only)

These modules have **placeholder pages** with `<PrototypePlaceholder />` component:

| Module | Treatment |
|---|---|
| `/org/tasks` aggregate board | Placeholder page, no fake board, no API calls |
| Reports page | Placeholder page |
| Finance page | Placeholder page |
| Resources page | Placeholder page |
| EventRating UI | No UI in base prototype |

### EXCLUDED (No Route, No Page, No Service)

| Module | Treatment |
|---|---|
| Posts | No route, no page, no service, no contract |
| Comments | No route, no page, no service, no contract |
| Messages/Chat working module | Placeholder page only if visible in nav |
| Finance-specific ledger/payment/budget logic | Not in scope |

---

## 6. Cross-Layer Traceability Requirement

Every **CORE** and **SUPPORTING** module must have complete cross-layer traceability:

```
Domain Entity
  ↓
Shared Contract (Request/Response DTOs)
  ↓
Backend Feature Skeleton (Endpoint placeholders + TODO notes)
  ↓
Frontend Service (API call stubs + TODO notes)
  ↓
Frontend Adapter (DTO → ViewModel mapping + TODO notes)
  ↓
Frontend Page/Component (UI skeleton + TODO notes)
  ↓
Permission Gating (Permission keys documented)
  ↓
TODO Implementation Notes (Detailed guidance)
```

Phase 3C must create documentation that traces each module through all layers.

---

## 7. Backend Skeleton Requirements

### 7.1 Backend Feature Folder Structure

For each **CORE** and **SUPPORTING** module, create:

```
backend/Org.Backend/Features/<Module>/
├── README.md                    # Module overview + TODO notes
├── Endpoints/
│   ├── <Action>Endpoint.cs.TODO # Endpoint skeleton with TODO
│   └── README.md                # Endpoint plan notes
├── Services/
│   └── README.md                # Service plan notes
├── Validators/
│   └── README.md                # Validation plan notes
└── Mappings/
    └── README.md                # Mapping plan notes
```

### 7.2 Backend Skeleton Rules

1. **No real FastEndpoints implementations** - only `.TODO` files with structure notes
2. **No database access code** - only TODO comments about what queries are needed
3. **No business logic** - only TODO comments about what logic is required
4. **Permission notes** - document which permission keys are required
5. **Validation notes** - document what validation rules are needed
6. **Error handling notes** - document expected error scenarios

### 7.3 DB_FOUNDATION_ONLY Backend Treatment

For EventMembers, Attendees, DigitalAssets, EventRatings, EventReports, Resources, ActivityHistory:

- Create `backend/Org.Backend/Features/<Module>/README.md` with:
  - "DB_FOUNDATION_ONLY - No working endpoint in base prototype"
  - Entity purpose and domain role
  - Future endpoint plan (not implemented)
  - Contract notes (not implemented)

---

## 8. Shared Contract Requirements

### 8.1 Contract Folder Structure

For each **CORE** and **SUPPORTING** module, create:

```
backend/Org.Shared/Features/<Module>/
├── <Module>Contracts.cs.TODO    # Request/Response DTO skeletons
└── README.md                    # Contract notes + TODO
```

### 8.2 Contract Skeleton Rules

1. **No real DTO implementations** - only `.TODO` files with structure notes
2. **Document required fields** - based on domain entities and API needs
3. **Document optional fields** - based on FINAL CLEAN blueprint
4. **Document validation rules** - what validation is needed
5. **Document response shapes** - list wrappers, detail responses, error responses

### 8.3 DB_FOUNDATION_ONLY Contract Treatment

For EventMembers, Attendees, DigitalAssets, EventRatings, EventReports, Resources, ActivityHistory:

- Create `backend/Org.Shared/Features/<Module>/README.md` with:
  - "DB_FOUNDATION_ONLY - No contract in base prototype"
  - Future contract plan (not implemented)
  - DTO shape notes (not implemented)

---

## 9. Frontend Skeleton Requirements

### 9.1 Frontend Folder Structure

For each **CORE** and **SUPPORTING** module, create:

```
frontend/org-frontend/src/
├── services/
│   └── <module>Service.js       # API call stubs + TODO
├── adapters/
│   └── <module>Adapter.js       # DTO → ViewModel mapping + TODO
├── pages/
│   └── <module>/
│       └── <Page>.jsx           # Page shell + TODO
└── components/
    └── <module>/
        └── <Component>.jsx      # Component shell + TODO
```

### 9.2 Frontend Skeleton Rules

1. **No real API calls** - only TODO stubs in services
2. **No mock data** - no fake data, no fake success
3. **No fake data** - adapters return empty/null safely
4. **Route skeleton** - routes defined in `AppRouter.jsx`
5. **Page shells** - minimal JSX structure with TODO comments
6. **Service stubs** - function signatures with TODO comments
7. **Adapter stubs** - mapping function signatures with TODO comments
8. **Component shells** - minimal JSX structure with TODO comments

### 9.3 PROTOTYPE_ONLY Frontend Treatment

For `/org/tasks` aggregate, Reports, Finance, Resources:

- Create page file with `<PrototypePlaceholder />` component
- No service file
- No adapter file
- No API call stubs
- Route defined but renders placeholder only

### 9.4 EXCLUDED Frontend Treatment

For Posts, Comments:

- No route
- No page
- No service
- No adapter
- No components

---

## 10. EventDetail Tree Requirement

The EventDetail tree is **CRITICAL** and must be detailed in Phase 3C skeleton.

### 10.1 EventDetail Component Hierarchy

```
OrgEventDetailPage
├── EventInfoSection (event metadata)
├── MilestonePanel (list of milestones)
│   └── CategoryPanel (list of categories per milestone)
│       └── TaskCard (list of tasks per category)
│           ├── TaskStatusControl
│           └── TaskAssignControl
├── MilestoneFormModal (create/edit milestone)
├── CategoryFormModal (create/edit category)
└── TaskFormModal (create/edit task)
```

### 10.2 EventDetail State Management Logic (TODO Notes)

Future implementation logic (document in TODO comments, do NOT implement):

1. **Load event**: `eventId` from `useParams()`, `orgId` from `useSearchParams()`
2. **Load event data**: `GET /events/{eventId}`
3. **Load milestones**: `GET /events/{eventId}/milestones`
4. **Load categories**: `GET /milestones/{milestoneId}/categories`
5. **Category DTO tasks handling**:
   - If category DTO has `tasks[]` → use it
   - If category DTO lacks `tasks[]` → initialize `category.tasks = []`
   - **Do NOT invent** a list-by-category task endpoint
6. **Create task success**: On `POST /categories/{categoryId}/tasks` success with `TaskDto` → append to local `category.tasks[]`
7. **Update/status/assign task**: On success → mutate tree state at page/hook level
8. **Delete task**: On success → remove from local `category.tasks[]`
9. **TaskCard must NOT own source-of-truth state** - state lives at page/hook level

### 10.3 EventDetail Skeleton Files

Create these files with TODO notes (no implementation):

```
frontend/org-frontend/src/
├── pages/org/OrgEventDetailPage.jsx
├── components/event-detail/
│   ├── MilestonePanel.jsx
│   ├── CategoryPanel.jsx
│   ├── TaskCard.jsx
│   ├── TaskStatusControl.jsx
│   ├── TaskAssignControl.jsx
│   ├── MilestoneFormModal.jsx
│   ├── CategoryFormModal.jsx
│   └── TaskFormModal.jsx
├── services/
│   ├── eventService.js
│   ├── milestoneService.js
│   ├── categoryService.js
│   └── taskService.js
└── adapters/
    ├── eventAdapter.js
    ├── milestoneAdapter.js
    ├── categoryAdapter.js
    └── taskAdapter.js
```

---

## 11. Route Rules

### 11.1 VITE_API_BASE_URL Convention

```env
VITE_API_BASE_URL=http://localhost:5000/api
```

`VITE_API_BASE_URL` **already includes `/api`**.

Service paths **must NOT** include `/api`:

```js
// ✅ Correct
httpClient.get('/organizations');
httpClient.get(`/organizations/${orgId}/events`);

// ❌ Wrong
httpClient.get('/api/organizations');
```

### 11.2 orgId Query String Rule

All `/org/*` routes use query string `?orgId=`:

- **Do NOT** use `useParams()` for `orgId`
- **Use** `useSearchParams()` for `orgId`

```js
// ✅ Correct
const [searchParams] = useSearchParams();
const orgId = searchParams.get('orgId');

// ❌ Wrong
const { orgId } = useParams();
```

`useParams()` is **only** for resource IDs in path:

```
/org/events/:id?orgId=
```

- `id` → `useParams()`
- `orgId` → `useSearchParams()`

### 11.3 No Global /forbidden Redirect

- 403 errors are handled at page/route level
- Render `<ForbiddenState />` component
- **Do NOT** create a global `/forbidden` route unless blueprint specifies it

---

## 12. Permission Rules

### 12.1 Permission Fallback Safety

`GET /organizations/{id}/permissions/me` response shape is not fully confirmed.

`roleService.getMyPermissions(orgId)` must normalize to `string[]`.

```js
function normalizePermissions(response) {
  if (Array.isArray(response)) return response;
  if (Array.isArray(response?.permissionKeys)) return response.permissionKeys;
  if (Array.isArray(response?.permissions)) return response.permissions;
  if (Array.isArray(response?.data)) return response.data;
  if (Array.isArray(response?.data?.permissionKeys)) return response.data.permissionKeys;
  if (Array.isArray(response?.data?.permissions)) return response.data.permissions;

  console.warn('[roleService] Cannot parse permissions, using safe fallback');
  return [];
}
```

### 12.2 Permission Fallback Security Rules

**CRITICAL**: Fallback must NEVER grant workspace access:

- Fallback **must NOT** include `org.workspace.access`
- Fallback **must NOT** include write/manage permissions
- If permission parse fails, user sees public/readonly UI only
- `isMember` **must NOT** be inferred from fallback permissions
- Workspace access is only confirmed when backend returns valid permission/membership

### 12.3 getMyOrganizations Ownership

`getMyOrganizations()` belongs to **userService**, NOT organizationService.

```js
// ✅ Correct
import { getMyOrganizations } from '../services/userService';

// ❌ Wrong
import { getMyOrganizations } from '../services/organizationService';
```

### 12.4 Role Assignment Canonical Rule

Role assignment uses **RoleId** as canonical:

- `Member.RoleId` is the source of truth
- `MemberRole` enum is for hierarchy/default mapping only
- **Do NOT** persist `MemberRole` directly in `Member` entity
- Frontend must use `RoleId` for role assignment, not fake role GUID

---

## 13. Required Documentation in Later Tasks

Phase 3C later tasks (3C-2, 3C-3, 3C-4, 3C-5) must create/update these docs:

### 13.1 Cross-Layer Documentation

| Document | Purpose |
|---|---|
| `MODULE_FILE_MANIFEST.md` | Complete file list per module across all layers |
| `API_CONTRACT_TODO_MAP.md` | Endpoint → Contract → Service → Page mapping |
| `TODO_IMPLEMENTATION_GUIDE.md` | Detailed implementation guidance for each TODO |
| `PROTOTYPE_ONLY_BOUNDARY.md` | Clear boundary between working and placeholder modules |
| `CROSS_LAYER_TRACEABILITY.md` | Entity → Contract → Backend → Frontend → Permission trace |

### 13.2 Phase 3C Completion Report

| Document | Purpose |
|---|---|
| `PHASE_3C_PROTOTYPE_SKELETON_REPORT.md` | Summary of Phase 3C completion, files created, decisions made |

---

## 14. Build/Verification Requirements

### 14.1 Build Verification for Later Tasks

Later Phase 3C tasks (3C-2, 3C-3, 3C-4, 3C-5) must run:

```powershell
# Backend build
dotnet build PBL3-rescue.sln

# Frontend build
cd frontend/org-frontend
npm run build
```

Both builds must pass with **0 errors**.

### 14.2 Build Verification for This Task (3C-1)

This task (3C-1) is **documentation only**. Build verification is not required unless documentation changes somehow require it.

---

## 15. Summary

Phase 3C creates the **architecture and prototype skeleton handoff** with:

1. ✅ Backend feature skeleton with TODO notes (no implementation)
2. ✅ Shared contract skeleton with DTO notes (no implementation)
3. ✅ Frontend route/page/service/adapter/component skeleton (no implementation)
4. ✅ EventDetail tree component skeleton (no implementation)
5. ✅ Cross-layer documentation and traceability
6. ✅ Detailed TODO implementation comments
7. ✅ Clear boundaries between CORE, SUPPORTING, DB_FOUNDATION_ONLY, PROTOTYPE_ONLY, and EXCLUDED modules

Phase 3C is **NOT** implementation. It is the handoff plan that will guide future implementation work.

---

**End of PHASE_3C_REQUIREMENTS_SPEC.md**
