# FE Live Integration Plan (Phase 2B)

- Phase scope: switch Frontend behavior to reliable live API usage without backend/schema changes.
- Canonical API source: `Docs/swagger-live.json` (100 operations, exported 2026-05-06).
- Evidence base: `Docs/FE_INTERFACE_SERVICE_MAP.md`, `Docs/FE_API_USAGE_MAP.md`, `Docs/API_LIVE_CONTRACT_MATRIX.md`.

## 1. Config Switch

### Current state
- `src/Org.Frontend/appsettings.json`: `FrontendData:UseMockServices = false`
- `src/Org.Frontend/appsettings.Development.json`: `FrontendData:UseMockServices = false`
- Startup logs also print effective value in `src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs`.

### Required Phase 2B behavior
- Keep `UseMockServices=false` for live-wiring work.
- Do not rely on mock UI output as correctness proof.

### Verify real HTTP calls happen
1. Start backend `http://localhost:5058`.
2. Start frontend and login.
3. Use browser Network tab and verify requests hit `/api/*` backend routes (not mock JSON).
4. Confirm these calls appear on first login flow:
- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/organizations/default`

Rollback risk:
- If login bootstrap blocks on a broken live-only page, keep `UseMockServices=false` and temporarily hide that page route instead of re-enabling mocks globally.

## 2. Service Wiring Check

Evidence file: `src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs`

### Interfaces correctly switched by `UseMockServices` when `false`
- `IAuthService -> AuthApiClient`
- `IOrganizationService -> OrganizationServiceApiClient`
- `IOrganizationContext -> OrganizationApiClient`
- `IMemberService -> MemberApiClient`
- `IDepartmentService -> DepartmentApiClient`
- `IEventService -> EventApiClient`
- `IMilestoneService -> MilestoneApiClient`
- `IEventCategoryService -> EventCategoryApiClient`
- `ITaskService -> TaskApiClient`
- `IRequestService -> RequestApiClient`
- `INotificationService -> NotificationService`

### Services still unsafe in live mode
- `IPostService` is hard-wired to `PostMockService` in both branches.
- `IOverviewService -> OverviewApiClient` but method throws `NotSupportedException`.
- `IMessageService -> MessageApiClient` but methods throw `NotSupportedException`.
- `IFriendService -> FriendApiClient` but methods throw `NotSupportedException`.
- `IDiscoverService -> DiscoverApiClient` but methods throw `NotSupportedException`.

Rollback risk:
- Broken routes from unsupported clients can break demo flow; mitigate by route hiding/disable, not by global mock switch.

## 3. Fix Batch Order (Phase 2B)

## 2B.1 Auth + live mode bootstrap

Files to modify:
- `src/Org.Frontend/Components/Pages/Auth/Login.razor`
- `src/Org.Frontend/Services/Auth/*`
- `src/Org.Frontend/Infrastructure/Auth/FrontendAuthStateProvider.cs`

APIs involved:
- `POST /api/auth/login`
- `GET /api/auth/me`

UI pages:
- Login and app bootstrap auth state.

Expected behavior:
- Successful login persists bearer token and fetches `me` profile.

Test steps:
1. Login as `example1@gmail.com / example1`.
2. Verify redirect and authenticated menu state.

Rollback risk:
- Token storage/expiry regressions can lock user out; keep old auth storage fallback path until verified.

## 2B.2 Organization context/default org

Files to modify:
- `src/Org.Frontend/Services/Organizations/OrganizationApiClient.cs`
- `src/Org.Frontend/Services/Organizations/IOrganizationContext.cs`
- route guard files under `Components/Layout`.

APIs involved:
- `GET /api/organizations/default`

UI pages:
- all `/org/*` routes dependent on `CurrentOrganizationId`.

Expected behavior:
- org context resolves once and is reused safely.

Test steps:
1. Open `/org/events`, `/org/members`, `/org/departments` after fresh login.
2. Verify no repeated context failures and no `Guid.Empty` fallback.

Rollback risk:
- invalid cached org id can cascade failures to all organization pages.

## 2B.3 Organization overview

Files to modify:
- `src/Org.Frontend/Services/Organizations/OrganizationServiceApiClient.cs`
- `src/Org.Frontend/Components/Pages/Organizations/OrganizationOverview.razor`

APIs involved:
- `GET /api/organizations/{id}/public-overview`
- `GET /api/organizations/{id}/permissions/me`
- `GET /api/organizations/{orgId}/departments`
- `GET /api/organizations/{orgId}/members`
- `GET /api/organizations/{orgId}/events`

UI pages:
- `/org-overview`

Expected behavior:
- overview sections (departments, leadership, highlights) rendered from live composite calls with null guards.

Test steps:
1. Load overview.
2. Verify no null exceptions and no empty placeholders caused by missing legacy fields.

Rollback risk:
- over-fetch composition can increase latency; cache lightweight sub-results.

## 2B.4 Members + role assignment

Files to modify:
- `src/Org.Frontend/Services/Members/MemberApiClient.cs`
- `src/Org.Frontend/Services/Organizations/OrganizationRoleApiClient.cs`
- `src/Org.Frontend/Components/Pages/Members/MemberList.razor`

APIs involved:
- `GET /api/organizations/{orgId}/members`
- `POST /api/organizations/{id}/members/{memberId}/role`
- `GET /api/organizations/{id}/roles`
- `GET /api/organizations/{id}/permissions/me`

UI pages:
- `/org/members`

Expected behavior:
- Role assignment uses roleId flow only.
- Avoid enum bridge with fake GUID mapping.

Test steps:
1. Open role dialog.
2. Assign a role.
3. Reload list and verify role changed.

Rollback risk:
- role permission hierarchy checks may block operations if stale role lists are cached.

## 2B.5 Departments

Files to modify:
- `src/Org.Frontend/Services/Departments/DepartmentApiClient.cs`
- `src/Org.Frontend/Components/Pages/Departments/DepartmentList.razor`

APIs involved:
- `GET /api/organizations/{orgId}/departments`
- `POST /api/departments`
- `PUT /api/departments/{id}`
- `DELETE /api/departments/{id}`
- `PUT /api/departments/{id}/manager`
- `POST /api/departments/{id}/members/{memberId}`
- `DELETE /api/departments/{id}/members/{memberId}`

UI pages:
- `/org/departments`

Expected behavior:
- Department CRUD and assignment work live.
- Department task CRUD controls are hidden until backend contract exists.

Test steps:
1. Create/update/delete department.
2. Assign/remove member.

Rollback risk:
- accidental exposure of unsupported department-task buttons causes runtime exceptions.

## 2B.6 Events

Files to modify:
- `src/Org.Frontend/Services/Events/EventApiClient.cs`
- `src/Org.Frontend/Components/Pages/Events/EventList.razor`
- `src/Org.Frontend/Components/Pages/Events/PublicEventDetail.razor`

APIs involved:
- `GET /api/organizations/{orgId}/events`
- `GET /api/events/{id}`
- `GET /api/events/{id}/public`
- `POST /api/events`
- `PUT /api/events/{id}`
- `DELETE /api/events/{id}`

UI pages:
- `/org/events`, `/org/events/{eventId}`, `/events/{eventId}`

Expected behavior:
- Event status label mapping handles `Planning` explicitly.
- Register/Unregister controls disabled (no backend endpoint in Swagger).

Test steps:
1. Create event with Planning status.
2. Confirm status label renders correctly.
3. Open public event detail and ensure no unsupported registration action.

Rollback risk:
- wrong status mapping can mislead demo narrative and workflow state.

## 2B.7 Milestones / Categories / Tasks

Files to modify:
- `src/Org.Frontend/Services/Milestones/MilestoneApiClient.cs`
- `src/Org.Frontend/Services/EventCategories/EventCategoryApiClient.cs`
- `src/Org.Frontend/Services/Tasks/TaskApiClient.cs`
- `src/Org.Frontend/Components/Pages/Tasks/TaskBoard.razor`

APIs involved:
- `GET/POST/PUT/DELETE /api/events/{eventId}/milestones`
- `GET/POST/PUT/DELETE /api/milestones/{milestoneId}/categories` and `/api/categories/{id}`
- `GET/POST /api/categories/{categoryId}/tasks`
- `PUT /api/tasks/{taskId}/status`

UI pages:
- Event detail + task board.

Expected behavior:
- UI status logic aligned to enum values.
- Multi-assignee UI guarded because live API client accepts max one assignee.

Test steps:
1. Create milestone/category/task.
2. Move task status.
3. Verify persistence on reload.

Rollback risk:
- enum/string mismatch can silently fail updates or show stale columns.

## 2B.8 Requests / Notifications

Files to modify:
- `src/Org.Frontend/Services/Requests/RequestApiClient.cs`
- `src/Org.Frontend/Services/Notifications/NotificationService.cs`
- `src/Org.Frontend/Components/Pages/Requests/OrganizationRequests.razor`
- notification components in layout/header.

APIs involved:
- `GET /api/organizations/{id}/requests`
- `POST /api/organizations/{id}/requests`
- `POST /api/organizations/requests/{requestId}/review`
- `GET /api/notifications`
- `GET /api/notifications/unread-count`
- `PUT /api/notifications/{id}/read`
- `PUT /api/notifications/read-all`

UI pages:
- requests page and notification panel.

Expected behavior:
- Permission-gated request actions.
- Notification unread count syncs with read actions.

Test steps:
1. Submit request.
2. Approve/reject as authorized role.
3. Validate notification count updates.

Rollback risk:
- permission mismatches can present actions user cannot execute.

## 2B.9 Hide unsupported modules/actions

Files to modify:
- `src/Org.Frontend/Components/Layout/NavMenu.razor`
- `src/Org.Frontend/Components/Layout/MainLayout.razor`
- pages under `Components/Pages/User/*`, `Components/Pages/Finance/*`, `Components/Pages/Reports/*`, `Components/Pages/Resources/*`

APIs involved:
- none live-safe for these modules currently.

UI pages:
- Discover, Friends, Messages, Home overview, Finance, Reports, Resources, any post feed entry.

Expected behavior:
- Unsupported routes hidden/disabled or show clear coming-soon message.

Test steps:
1. Verify menu items are absent or disabled.
2. Direct URL should show guard/coming-soon page.

Rollback risk:
- partial hiding (menu only) still allows direct-route runtime failures.

## 4. Recommended Member Role Assignment Approach

Use approach B for demo safety:
- `POST /api/organizations/{id}/members/{memberId}/role` with `roleId`.

Reason:
- FE already uses this in `MemberList.razor` via `IOrganizationRoleService.AssignRoleToMemberAsync`.
- Avoids fake GUID -> enum conversion in `MemberApiClient.AssignRole` (`MapLegacyRoleId`) that can drift from real role catalog.

## 5. Exit Criteria for Phase 2B

1. All P0/P1 demo pages run with `UseMockServices=false` and no mock-only fallbacks in flow.
2. No `NotSupportedException` on reachable demo actions.
3. Core APIs in `Docs/API_LIVE_CONTRACT_MATRIX.md` pass manual checklist.
4. Unsupported modules are explicitly hidden/disabled and out of demo script.
