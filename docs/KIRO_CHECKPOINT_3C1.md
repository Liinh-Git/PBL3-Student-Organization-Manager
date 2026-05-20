# KIRO_CHECKPOINT_3C1

## Task Name
Phase 3C-1: Requirements Spec Only for Architecture + Prototype Skeleton Handoff

## Task Purpose
Create requirements specification and task breakdown documentation for Phase 3C without modifying any source code.

---

## Files Read

### Primary Source of Truth Files
1. `PBL3-rescue/docs/PBL3_SYSTEM_DESIGN_AND_PROTOTYPE_HANDOFF_FINAL_CLEAN.md` - Highest priority blueprint
2. `PBL3-rescue/docs/DOMAIN_ENTITY_LOCK_V1.md` - Domain model specification (22 entities, 21 enums)
3. `PBL3-rescue/docs/PHASE_3B2_DOMAIN_APPLY_REPORT.md` - Phase 3B.2 completion status
4. `PBL3-rescue/docs/PHASE_3_SCOPE_LOCK.md` - Phase 3 scope and restrictions
5. `PBL3-rescue/docs/DO_NOT_IMPLEMENT_YET.md` - Forbidden implementation items

### Supporting Files
6. `PBL3-rescue/docs/PHASE_3B1_DOMAIN_LOCK_REPORT.md` - Phase 3B.1 completion status
7. `PBL3-rescue/docs/PHASE_3A_REPO_FOUNDATION_REPORT.md` - Phase 3A completion status
8. `PBL3-rescue/docs/REPO_STRUCTURE_LOCK.md` - Repository structure specification

---

## Files Created

### Documentation Files
1. ✅ `PBL3-rescue/docs/PHASE_3C_REQUIREMENTS_SPEC.md` (15 sections, ~500 lines)
   - Purpose and current state
   - Absolute non-goals
   - Core system chain
   - Module status classification (CORE, SUPPORTING, DB_FOUNDATION_ONLY, PROTOTYPE_ONLY, EXCLUDED)
   - Cross-layer traceability requirements
   - Backend skeleton requirements
   - Shared contract requirements
   - Frontend skeleton requirements
   - EventDetail tree requirements
   - Route rules (VITE_API_BASE_URL, orgId query string, no global /forbidden)
   - Permission rules (fallback safety, getMyOrganizations ownership, role assignment canonical)
   - Required documentation for later tasks
   - Build/verification requirements

2. ✅ `PBL3-rescue/docs/PHASE_3C_TASK_BREAKDOWN.md` (~400 lines)
   - Task 3C-1: Requirements Spec Only (current task)
   - Task 3C-2: Backend Feature Skeleton Only
   - Task 3C-3: Shared Contract Skeleton Only
   - Task 3C-4: Frontend Skeleton Only
   - Task 3C-5: Cross-layer Docs + Build Verification
   - Strict folder boundaries per task
   - Module lists per task
   - File structure templates
   - Verification requirements

3. ✅ `PBL3-rescue/docs/KIRO_CHECKPOINT_3C1.md` (this file)
   - Files read
   - Files created
   - Important decisions
   - What was NOT done
   - Recommended next task
   - Warnings for next task

---

## Important Decisions

### 1. Module Classification

**CORE Modules** (12 modules - full skeleton required):
- Auth, Users, Organizations, Members, Departments, Events, Milestones, EventCategories, Tasks, Requests, Notifications, RolesPermissions

**SUPPORTING Modules** (3 modules - full skeleton required):
- Friends, Discover, User Profile/Settings

**DB_FOUNDATION_ONLY** (7 entities - README.md notes only, no working UI/API):
- EventMembers, Attendees, DigitalAssets, EventRatings, EventReports, Resources, ActivityHistory

**PROTOTYPE_ONLY** (4 modules - placeholder pages only):
- `/org/tasks` aggregate board, Reports page, Finance page, Resources page

**EXCLUDED** (no route, no page, no service):
- Posts, Comments, Messages/Chat working module, Finance-specific ledger/payment/budget logic

### 2. Task is CORE Inside EventDetail

**Critical Clarification**: Task module is **CORE** inside the EventDetail tree:
```
Event → Milestone → EventCategory → Task
```

**Only `/org/tasks` aggregate board** is PROTOTYPE_ONLY (placeholder page, no API calls).

This distinction is critical to prevent confusion about Task scope.

### 3. EventMember and Attendee Treatment

**EventMember** and **Attendee** are included in DB v1 (Phase 3B.2 completed) to preserve domain integrity, but:
- No working UI in base prototype
- No working API in base prototype
- Backend/contract notes only in Phase 3C
- DB_FOUNDATION_ONLY classification

### 4. EventDetail Tree State Management

Documented detailed TODO logic for future implementation:
1. Load event: `eventId` from `useParams()`, `orgId` from `useSearchParams()`
2. Load milestones: `GET /events/{eventId}/milestones`
3. Load categories: `GET /milestones/{milestoneId}/categories`
4. Category DTO tasks handling:
   - If category DTO has `tasks[]` → use it
   - If category DTO lacks `tasks[]` → initialize `category.tasks = []`
   - **Do NOT invent** a list-by-category task endpoint
5. Create task success: Append to local `category.tasks[]`
6. Update/status/assign/delete: Mutate tree state at page/hook level
7. **TaskCard must NOT own source-of-truth state**

### 5. Permission Fallback Safety

Documented critical security rule:
- Permission fallback **must NEVER** grant `org.workspace.access`
- Permission fallback **must NEVER** grant write/manage permissions
- If permission parse fails, user sees public/readonly UI only
- `isMember` **must NOT** be inferred from fallback permissions

### 6. Route Conventions

**VITE_API_BASE_URL Convention**:
- `VITE_API_BASE_URL=http://localhost:5000/api` (already includes `/api`)
- Service paths **must NOT** include `/api`
- Correct: `httpClient.get('/organizations')`
- Wrong: `httpClient.get('/api/organizations')`

**orgId Query String Rule**:
- All `/org/*` routes use query string `?orgId=`
- Use `useSearchParams()` for `orgId`, NOT `useParams()`
- `useParams()` only for resource IDs in path (e.g., `/org/events/:id`)

**No Global /forbidden Redirect**:
- 403 errors handled at page/route level
- Render `<ForbiddenState />` component
- Do NOT create global `/forbidden` route unless blueprint specifies it

### 7. getMyOrganizations Ownership

`getMyOrganizations()` belongs to **userService**, NOT organizationService.

### 8. Role Assignment Canonical Rule

Role assignment uses **RoleId** as canonical:
- `Member.RoleId` is source of truth
- `MemberRole` enum is for hierarchy/default mapping only
- Do NOT persist `MemberRole` directly in `Member` entity

### 9. Task Breakdown Strategy

Split Phase 3C into **5 small, Kiro-safe tasks** with strict folder boundaries:
- 3C-1: Requirements Spec Only (docs only)
- 3C-2: Backend Feature Skeleton Only (backend/Org.Backend/Features only)
- 3C-3: Shared Contract Skeleton Only (backend/Org.Shared only)
- 3C-4: Frontend Skeleton Only (frontend/org-frontend/src only)
- 3C-5: Cross-layer Docs + Build Verification (docs + build fixes only)

This prevents accidental modifications to completed work.

---

## What Was NOT Done

### Source Code
- ❌ No backend feature skeleton created (will be done in 3C-2)
- ❌ No shared contract skeleton created (will be done in 3C-3)
- ❌ No frontend skeleton created (will be done in 3C-4)
- ❌ No cross-layer documentation created (will be done in 3C-5)

### Domain/Infrastructure
- ❌ No modifications to `backend/Org.Backend/Domain/` (already completed in Phase 3B.2)
- ❌ No modifications to `backend/Org.Backend/Infrastructure/Persistence/` (already completed in Phase 3B.2)
- ❌ No modifications to `backend/Org.Backend/Migrations/` (paused, not in Phase 3C scope)

### Database Operations
- ❌ No migration creation
- ❌ No database update
- ❌ No seeding
- ❌ No database connection

### Implementation
- ❌ No real FastEndpoints implementations
- ❌ No real database access code
- ❌ No real business logic
- ❌ No real API calls
- ❌ No mock data
- ❌ No fake data

### Build Verification
- ❌ No build verification run (not required for documentation-only task)

---

## Recommended Next Task

**Task 3C-2: Backend Feature Skeleton Only**

### Purpose
Create backend feature skeleton with TODO notes for all CORE and SUPPORTING modules.

### Allowed Folders
- `backend/Org.Backend/Features/` (create only)
- `docs/` (create/update only)

### Forbidden Folders
- `backend/Org.Backend/Domain/` (already completed, do NOT modify)
- `backend/Org.Backend/Infrastructure/Persistence/` (already completed, do NOT modify)
- `backend/Org.Shared/` (will be created in 3C-3)
- `frontend/org-frontend/src/` (will be created in 3C-4)
- `backend/Org.Backend/Migrations/` (paused, not in Phase 3C scope)

### Modules to Create
- **CORE** (12 modules): Auth, Users, Organizations, Members, Departments, Events, Milestones, EventCategories, Tasks, Requests, Notifications, RolesPermissions
- **SUPPORTING** (2 modules): Friends, Discover
- **DB_FOUNDATION_ONLY** (7 modules): EventMembers, Attendees, DigitalAssets, EventRatings, EventReports, Resources, ActivityHistory (README.md only)

### Output
- Backend feature skeleton files (README.md and .TODO files)
- `docs/KIRO_CHECKPOINT_3C2.md`

### Verification
- Run `dotnet build PBL3-rescue.sln` (must pass with 0 errors)
- Confirm no Domain/ or Infrastructure/Persistence/ modifications

---

## Warnings for Next Task (3C-2)

### Critical Warnings

1. **Do NOT modify Domain/ or Infrastructure/Persistence/**
   - These folders were completed in Phase 3B.2
   - Any modifications will break the domain model
   - Only create files in `backend/Org.Backend/Features/`

2. **Do NOT create real implementations**
   - Only create `.TODO` files with structure notes
   - Only create README.md files with TODO notes
   - No real FastEndpoints handler code
   - No real database access code
   - No real business logic

3. **Do NOT create migrations**
   - Migrations are paused
   - Do NOT run `dotnet ef migrations add`
   - Do NOT run `dotnet ef database update`

4. **Do NOT modify Org.Shared yet**
   - Shared contracts will be created in 3C-3
   - Do NOT create contract files in 3C-2

5. **Do NOT create frontend files yet**
   - Frontend skeleton will be created in 3C-4
   - Do NOT create frontend files in 3C-2

### Module-Specific Warnings

1. **Task Module**
   - Task is CORE inside EventDetail tree
   - Create full backend skeleton for Task module
   - Do NOT confuse with `/org/tasks` aggregate board (PROTOTYPE_ONLY)

2. **EventMember and Attendee**
   - DB_FOUNDATION_ONLY classification
   - Create README.md only (no endpoint skeleton)
   - Document "DB_FOUNDATION_ONLY - No working endpoint in base prototype"

3. **DB_FOUNDATION_ONLY Modules**
   - EventMembers, Attendees, DigitalAssets, EventRatings, EventReports, Resources, ActivityHistory
   - Create README.md only (no endpoint skeleton)
   - Document future endpoint plan (not implemented)

### File Structure Warnings

1. **CORE and SUPPORTING modules** must have:
   ```
   backend/Org.Backend/Features/<Module>/
   ├── README.md
   ├── Endpoints/
   │   ├── <Action>Endpoint.cs.TODO
   │   └── README.md
   ├── Services/
   │   └── README.md
   ├── Validators/
   │   └── README.md
   └── Mappings/
       └── README.md
   ```

2. **DB_FOUNDATION_ONLY modules** must have:
   ```
   backend/Org.Backend/Features/<Module>/
   └── README.md  # "DB_FOUNDATION_ONLY" notes only
   ```

### Build Verification Warning

After completing 3C-2, **MUST** run:
```powershell
dotnet build PBL3-rescue.sln
```

Build must pass with **0 errors**. If build fails:
- Fix only build-breaking issues
- Do NOT add implementations
- Document fixes in checkpoint report

---

## Confirmation

✅ **Task 3C-1 completed successfully**

- All 3 documentation files created
- No source code modified
- No database operations performed
- No implementations created
- Ready for Task 3C-2

---

**End of KIRO_CHECKPOINT_3C1.md**
