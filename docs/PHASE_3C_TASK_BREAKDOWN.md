# PHASE_3C_TASK_BREAKDOWN

## Overview

Phase 3C is split into **5 small, Kiro-safe tasks** to create the architecture and prototype skeleton handoff. Each task has strict folder boundaries to prevent accidental modifications to completed work.

---

## Task 3C-1: Requirements Spec Only ✅ CURRENT TASK

**Status**: IN PROGRESS

**Purpose**: Create requirements specification and task breakdown for Phase 3C.

**Allowed Folders**:
- `docs/` (create/update only)

**Forbidden Folders**:
- `backend/Org.Backend/Domain/` (already completed in Phase 3B.2)
- `backend/Org.Backend/Infrastructure/Persistence/` (already completed in Phase 3B.2)
- `backend/Org.Backend/Features/` (will be created in 3C-2)
- `backend/Org.Shared/` (will be created in 3C-3)
- `frontend/org-frontend/src/` (will be created in 3C-4)
- `backend/Org.Backend/Migrations/` (paused, not in Phase 3C scope)

**Output Files**:
1. `docs/PHASE_3C_REQUIREMENTS_SPEC.md` - Complete requirements specification
2. `docs/PHASE_3C_TASK_BREAKDOWN.md` - This file
3. `docs/KIRO_CHECKPOINT_3C1.md` - Checkpoint report

**What This Task Does**:
- Read source of truth files
- Create requirements specification
- Create task breakdown
- Create checkpoint report
- **NO source code modifications**

**What This Task Does NOT Do**:
- ❌ No backend code creation
- ❌ No frontend code creation
- ❌ No contract creation
- ❌ No migrations
- ❌ No database operations

**Verification**:
- Confirm all 3 docs created
- Confirm no source code modified

**Next Task**: 3C-2 Backend Feature Skeleton Only

---

## Task 3C-2: Backend Feature Skeleton Only

**Purpose**: Create backend feature skeleton with TODO notes for all CORE and SUPPORTING modules.

**Allowed Folders**:
- `backend/Org.Backend/Features/` (create only)
- `docs/` (create/update only)

**Forbidden Folders**:
- `backend/Org.Backend/Domain/` (already completed, do NOT modify)
- `backend/Org.Backend/Infrastructure/Persistence/` (already completed, do NOT modify)
- `backend/Org.Shared/` (will be created in 3C-3)
- `frontend/org-frontend/src/` (will be created in 3C-4)
- `backend/Org.Backend/Migrations/` (paused, not in Phase 3C scope)

**Modules to Create**:

### CORE Modules (Full Skeleton)
1. Auth
2. Users
3. Organizations
4. Members
5. Departments
6. Events
7. Milestones
8. EventCategories
9. Tasks
10. Requests
11. Notifications
12. RolesPermissions

### SUPPORTING Modules (Full Skeleton)
13. Friends
14. Discover

### DB_FOUNDATION_ONLY (Notes Only)
15. EventMembers (README.md only)
16. Attendees (README.md only)
17. DigitalAssets (README.md only)
18. EventRatings (README.md only)
19. EventReports (README.md only)
20. Resources (README.md only)
21. ActivityHistory (README.md only)

**File Structure Per Module** (CORE and SUPPORTING only):

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

**File Structure Per Module** (DB_FOUNDATION_ONLY):

```
backend/Org.Backend/Features/<Module>/
└── README.md                    # "DB_FOUNDATION_ONLY" notes
```

**Output Files**:
- Backend feature skeleton files (README.md and .TODO files)
- `docs/KIRO_CHECKPOINT_3C2.md` - Checkpoint report

**Rules**:
1. **No real FastEndpoints implementations** - only `.TODO` files
2. **No database access code** - only TODO comments
3. **No business logic** - only TODO comments
4. **Permission notes** - document required permission keys
5. **Validation notes** - document validation rules
6. **Error handling notes** - document error scenarios

**Verification**:
- Run `dotnet build PBL3-rescue.sln` (must pass with 0 errors)
- Confirm no Domain/ or Infrastructure/Persistence/ modifications
- Confirm all CORE/SUPPORTING modules have full skeleton
- Confirm all DB_FOUNDATION_ONLY modules have README.md only

**Next Task**: 3C-3 Shared Contract Skeleton Only

---

## Task 3C-3: Shared Contract Skeleton Only

**Purpose**: Create shared contract skeleton with DTO notes for all CORE and SUPPORTING modules.

**Allowed Folders**:
- `backend/Org.Shared/Common/` (create/update only)
- `backend/Org.Shared/Features/` (create only)
- `docs/` (create/update only)

**Forbidden Folders**:
- `backend/Org.Backend/Domain/` (already completed, do NOT modify)
- `backend/Org.Backend/Infrastructure/Persistence/` (already completed, do NOT modify)
- `backend/Org.Backend/Features/` (already completed in 3C-2, do NOT modify)
- `frontend/org-frontend/src/` (will be created in 3C-4)
- `backend/Org.Backend/Migrations/` (paused, not in Phase 3C scope)

**Modules to Create**:

### CORE Modules (Full Contract Skeleton)
1. Auth
2. Users
3. Organizations
4. Members
5. Departments
6. Events
7. Milestones
8. EventCategories
9. Tasks
10. Requests
11. Notifications
12. RolesPermissions

### SUPPORTING Modules (Full Contract Skeleton)
13. Friends
14. Discover

### DB_FOUNDATION_ONLY (Notes Only)
15. EventMembers (README.md only)
16. Attendees (README.md only)
17. DigitalAssets (README.md only)
18. EventRatings (README.md only)
19. EventReports (README.md only)
20. Resources (README.md only)
21. ActivityHistory (README.md only)

**File Structure Per Module** (CORE and SUPPORTING only):

```
backend/Org.Shared/Features/<Module>/
├── <Module>Contracts.cs.TODO    # Request/Response DTO skeletons
└── README.md                    # Contract notes + TODO
```

**File Structure Per Module** (DB_FOUNDATION_ONLY):

```
backend/Org.Shared/Features/<Module>/
└── README.md                    # "DB_FOUNDATION_ONLY" notes
```

**Common Contracts**:

```
backend/Org.Shared/Common/
├── ApiResponse.cs.TODO          # Generic API response wrapper
├── ListResponse.cs.TODO         # List response wrapper
├── ErrorResponse.cs.TODO        # Error response shape
└── README.md                    # Common contract notes
```

**Output Files**:
- Shared contract skeleton files (README.md and .TODO files)
- `docs/KIRO_CHECKPOINT_3C3.md` - Checkpoint report

**Rules**:
1. **No real DTO implementations** - only `.TODO` files
2. **Document required fields** - based on domain entities
3. **Document optional fields** - based on FINAL CLEAN blueprint
4. **Document validation rules** - what validation is needed
5. **Document response shapes** - list wrappers, detail responses, error responses

**Verification**:
- Run `dotnet build PBL3-rescue.sln` (must pass with 0 errors)
- Confirm no Domain/ or Infrastructure/Persistence/ or Features/ modifications
- Confirm all CORE/SUPPORTING modules have contract skeleton
- Confirm all DB_FOUNDATION_ONLY modules have README.md only

**Next Task**: 3C-4 Frontend Skeleton Only

---

## Task 3C-4: Frontend Skeleton Only

**Purpose**: Create frontend React skeleton with route/page/service/adapter/component structure.

**Allowed Folders**:
- `frontend/org-frontend/src/` (create/update only)
- `frontend/org-frontend/.env.example` (update if needed)
- `docs/` (create/update only)

**Forbidden Folders**:
- `backend/` (already completed, do NOT modify)
- `backend/Org.Backend/Migrations/` (paused, not in Phase 3C scope)

**Modules to Create**:

### CORE Modules (Full Frontend Skeleton)
1. Auth (Login, Register pages)
2. Users (Profile, Settings, MyOrgs, MyEvents pages)
3. Organizations (Overview, CRUD)
4. Members (List, Add, Remove, Role Assignment)
5. Departments (List, CRUD, Manager, Members)
6. Events (List, CRUD, Detail, Visibility)
7. Milestones (Inside EventDetail)
8. EventCategories (Inside EventDetail)
9. Tasks (Inside EventDetail - CORE)
10. Requests (Submit, Review, Approve)
11. Notifications (Badge, List, Mark Read)
12. RolesPermissions (Role CRUD, Permission Gating)

### SUPPORTING Modules (Full Frontend Skeleton)
13. Friends (Friends List, Friend Requests)
14. Discover (Discover Orgs/Events)

### PROTOTYPE_ONLY (Placeholder Pages Only)
15. `/org/tasks` aggregate board (PrototypePlaceholder only)
16. Reports page (PrototypePlaceholder only)
17. Finance page (PrototypePlaceholder only)
18. Resources page (PrototypePlaceholder only)

### EXCLUDED (No Route, No Page, No Service)
- Posts (no route, no page, no service)
- Comments (no route, no page, no service)

**File Structure**:

```
frontend/org-frontend/src/
├── api/
│   └── httpClient.js            # Update if needed
├── contexts/
│   ├── AuthContext.jsx          # Update if needed
│   └── OrgContext.jsx           # Update if needed
├── hooks/
│   ├── useAuth.js               # Update if needed
│   ├── useOrg.js                # Update if needed
│   ├── usePermission.js         # Update if needed
│   └── useNotifications.js      # Create
├── services/
│   ├── authService.js           # Create with TODO stubs
│   ├── userService.js           # Create with TODO stubs
│   ├── organizationService.js   # Create with TODO stubs
│   ├── roleService.js           # Create with TODO stubs
│   ├── memberService.js         # Create with TODO stubs
│   ├── eventService.js          # Create with TODO stubs
│   ├── milestoneService.js      # Create with TODO stubs
│   ├── categoryService.js       # Create with TODO stubs
│   ├── taskService.js           # Create with TODO stubs
│   ├── departmentService.js     # Create with TODO stubs
│   ├── notificationService.js   # Create with TODO stubs
│   ├── requestService.js        # Create with TODO stubs
│   ├── friendService.js         # Create with TODO stubs
│   └── discoverService.js       # Create with TODO stubs
├── adapters/
│   ├── userAdapter.js           # Create with TODO stubs
│   ├── organizationAdapter.js   # Create with TODO stubs
│   ├── eventAdapter.js          # Create with TODO stubs
│   ├── milestoneAdapter.js      # Create with TODO stubs
│   ├── categoryAdapter.js       # Create with TODO stubs
│   ├── taskAdapter.js           # Create with TODO stubs
│   ├── memberAdapter.js         # Create with TODO stubs
│   ├── departmentAdapter.js     # Create with TODO stubs
│   ├── notificationAdapter.js   # Create with TODO stubs
│   └── requestAdapter.js        # Create with TODO stubs
├── router/
│   ├── AppRouter.jsx            # Create with route skeleton
│   ├── ProtectedRoute.jsx       # Create with TODO
│   └── OrgMemberRoute.jsx       # Create with TODO
├── layouts/
│   ├── AppLayout.jsx            # Create with TODO
│   ├── PublicLayout.jsx         # Create with TODO
│   ├── Sidebar.jsx              # Create with TODO
│   └── TopBar.jsx               # Create with TODO
├── components/
│   ├── shared/
│   │   ├── LoadingSpinner.jsx   # Create
│   │   ├── EmptyState.jsx       # Create
│   │   ├── ErrorState.jsx       # Create
│   │   ├── ForbiddenState.jsx   # Create
│   │   ├── PrototypePlaceholder.jsx # Create
│   │   ├── ConfirmDialog.jsx    # Create with TODO
│   │   └── Pagination.jsx       # Create with TODO
│   ├── notifications/
│   │   └── NotificationBadge.jsx # Create with TODO
│   ├── org/
│   │   ├── OrgCard.jsx          # Create with TODO
│   │   └── OrgSwitcher.jsx      # Create with TODO
│   ├── event/
│   │   ├── EventCard.jsx        # Create with TODO
│   │   └── EventStatusBadge.jsx # Create with TODO
│   └── event-detail/
│       ├── MilestonePanel.jsx   # Create with TODO
│       ├── CategoryPanel.jsx    # Create with TODO
│       ├── TaskCard.jsx         # Create with TODO
│       ├── TaskStatusControl.jsx # Create with TODO
│       ├── TaskAssignControl.jsx # Create with TODO
│       ├── MilestoneFormModal.jsx # Create with TODO
│       ├── CategoryFormModal.jsx # Create with TODO
│       └── TaskFormModal.jsx    # Create with TODO
└── pages/
    ├── public/
    │   ├── HomePage.jsx         # Create with TODO
    │   ├── PublicEventsPage.jsx # Create with TODO
    │   └── PublicEventDetailPage.jsx # Create with TODO
    ├── auth/
    │   ├── LoginPage.jsx        # Create with TODO
    │   └── RegisterPage.jsx     # Create with TODO
    ├── user/
    │   ├── UserOrganizationsPage.jsx # Create with TODO
    │   ├── UserEventsPage.jsx   # Create with TODO
    │   ├── UserProfilePage.jsx  # Create with TODO
    │   ├── UserSettingsPage.jsx # Create with TODO
    │   ├── UserFriendsPage.jsx  # Create with TODO
    │   ├── UserDiscoverPage.jsx # Create with TODO
    │   └── UserMessagesPage.jsx # PROTOTYPE_ONLY placeholder
    └── org/
        ├── OrgOverviewPage.jsx  # Create with TODO
        ├── OrgMembersPage.jsx   # Create with TODO
        ├── OrgDepartmentsPage.jsx # Create with TODO
        ├── OrgEventsPage.jsx    # Create with TODO
        ├── OrgEventDetailPage.jsx # Create with TODO (EventDetail tree)
        ├── OrgRequestsPage.jsx  # Create with TODO
        ├── OrgRolesPage.jsx     # Create with TODO
        ├── OrgTasksPlaceholderPage.jsx # PROTOTYPE_ONLY placeholder
        ├── OrgFinancePage.jsx   # PROTOTYPE_ONLY placeholder
        ├── OrgReportsPage.jsx   # PROTOTYPE_ONLY placeholder
        └── OrgResourcesPage.jsx # PROTOTYPE_ONLY placeholder
```

**Output Files**:
- Frontend skeleton files (all files above)
- `docs/KIRO_CHECKPOINT_3C4.md` - Checkpoint report

**Rules**:
1. **No real API calls** - only TODO stubs in services
2. **No mock data** - no fake data, no fake success
3. **No fake data** - adapters return empty/null safely
4. **Route skeleton** - routes defined in `AppRouter.jsx`
5. **Page shells** - minimal JSX structure with TODO comments
6. **Service stubs** - function signatures with TODO comments
7. **Adapter stubs** - mapping function signatures with TODO comments
8. **Component shells** - minimal JSX structure with TODO comments
9. **PROTOTYPE_ONLY pages** - use `<PrototypePlaceholder />` component
10. **EXCLUDED modules** - no route, no page, no service

**EventDetail Tree Special Requirements**:

The EventDetail tree is **CRITICAL**. Create detailed TODO notes for:

1. Load event: `eventId` from `useParams()`, `orgId` from `useSearchParams()`
2. Load event data: `GET /events/{eventId}`
3. Load milestones: `GET /events/{eventId}/milestones`
4. Load categories: `GET /milestones/{milestoneId}/categories`
5. Category DTO tasks handling:
   - If category DTO has `tasks[]` → use it
   - If category DTO lacks `tasks[]` → initialize `category.tasks = []`
   - **Do NOT invent** a list-by-category task endpoint
6. Create task success: On `POST /categories/{categoryId}/tasks` success with `TaskDto` → append to local `category.tasks[]`
7. Update/status/assign task: On success → mutate tree state at page/hook level
8. Delete task: On success → remove from local `category.tasks[]`
9. **TaskCard must NOT own source-of-truth state** - state lives at page/hook level

**Verification**:
- Run `npm run build` in `frontend/org-frontend/` (must pass with 0 errors)
- Confirm no backend/ modifications
- Confirm all CORE/SUPPORTING modules have full frontend skeleton
- Confirm all PROTOTYPE_ONLY pages use `<PrototypePlaceholder />`
- Confirm no Posts/Comments routes/pages/services

**Next Task**: 3C-5 Cross-layer Docs + Build Verification

---

## Task 3C-5: Cross-layer Docs + Build Verification

**Purpose**: Create cross-layer documentation and verify final build.

**Allowed Folders**:
- `docs/` (create/update only)
- `backend/` (build-only fixes if necessary)
- `frontend/` (build-only fixes if necessary)

**Forbidden Folders**:
- No new feature creation
- No new contract creation
- No new frontend skeleton creation
- Only build-only fixes if absolutely necessary

**Output Files**:

1. `docs/MODULE_FILE_MANIFEST.md`
   - Complete file list per module across all layers
   - Backend feature files
   - Shared contract files
   - Frontend service/adapter/page/component files

2. `docs/API_CONTRACT_TODO_MAP.md`
   - Endpoint → Contract → Service → Page mapping
   - Cross-layer traceability for each API endpoint

3. `docs/TODO_IMPLEMENTATION_GUIDE.md`
   - Detailed implementation guidance for each TODO
   - Step-by-step instructions for future implementation

4. `docs/PROTOTYPE_ONLY_BOUNDARY.md`
   - Clear boundary between working and placeholder modules
   - CORE vs SUPPORTING vs DB_FOUNDATION_ONLY vs PROTOTYPE_ONLY vs EXCLUDED

5. `docs/CROSS_LAYER_TRACEABILITY.md`
   - Entity → Contract → Backend → Frontend → Permission trace
   - Complete traceability for each CORE and SUPPORTING module

6. `docs/PHASE_3C_PROTOTYPE_SKELETON_REPORT.md`
   - Summary of Phase 3C completion
   - Files created
   - Decisions made
   - Build verification results
   - Recommended next steps

7. `docs/KIRO_CHECKPOINT_3C5.md`
   - Checkpoint report for this task

**Build Verification**:

```powershell
# Backend build
dotnet build PBL3-rescue.sln

# Frontend build
cd frontend/org-frontend
npm run build
```

Both builds must pass with **0 errors**.

If build fails:
- Fix only the build-breaking issues
- Do NOT add new features
- Do NOT add new implementations
- Document fixes in checkpoint report

**Verification**:
- Confirm all 7 docs created
- Confirm backend build passes (0 errors)
- Confirm frontend build passes (0 errors)
- Confirm no new features added
- Confirm no implementations added

**Next Phase**: Phase 3D (Implementation) - NOT in Phase 3C scope

---

## Task Execution Order

1. ✅ **3C-1**: Requirements Spec Only (CURRENT TASK)
2. ⏳ **3C-2**: Backend Feature Skeleton Only
3. ⏳ **3C-3**: Shared Contract Skeleton Only
4. ⏳ **3C-4**: Frontend Skeleton Only
5. ⏳ **3C-5**: Cross-layer Docs + Build Verification

---

## Critical Rules for All Tasks

### Folder Boundaries
- Each task has strict allowed/forbidden folders
- **Do NOT** modify folders outside allowed scope
- **Do NOT** modify completed work from previous tasks

### No Implementation
- **No real FastEndpoints implementations**
- **No real database access code**
- **No real business logic**
- **No real API calls**
- **No mock data**
- **No fake data**

### TODO Notes Only
- All skeleton files have TODO comments
- TODO comments document future implementation
- TODO comments document required logic
- TODO comments document validation rules
- TODO comments document error handling

### Build Safety
- Each task must verify build passes
- Backend: `dotnet build PBL3-rescue.sln`
- Frontend: `npm run build` in `frontend/org-frontend/`
- Both builds must pass with **0 errors**

### Checkpoint Reports
- Each task creates a checkpoint report
- Checkpoint reports document:
  - Files read
  - Files created
  - Decisions made
  - What was NOT done
  - Recommended next task
  - Warnings for next task

---

**End of PHASE_3C_TASK_BREAKDOWN.md**
