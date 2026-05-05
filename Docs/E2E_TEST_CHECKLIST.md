# E2E Test Checklist (Phase 2B Execution)

- Execution date: 2026-05-06
- Backend base URL: `http://localhost:5058`
- Frontend base URL: `http://localhost:5236`
- Demo account: `example1@gmail.com / example1`
- Live mode config: `FrontendData:UseMockServices=false` in both frontend appsettings files.

## Status Summary

| Test Case | Status | Notes |
|---|---|---|
| TC-01 Login Success | PARTIAL | `POST /api/auth/login` verified by live API smoke; browser form flow still needs manual click-through. |
| TC-02 Auth Me Bootstrap | PARTIAL | `GET /api/auth/me` verified with bearer token; refresh/session restoration needs manual browser verification. |
| TC-03 Default Organization Context | PARTIAL | `GET /api/organizations/default` verified; org workspace navigation needs manual browser verification. |
| TC-04 Organization Overview | PARTIAL | Live enrichment code implemented; manual render verification pending. |
| TC-05 Member List Load | PARTIAL | Live member/department/role mapping path validated in code; manual page verification pending. |
| TC-06 Role Assignment | PARTIAL | Canonical roleId API path confirmed in `OrganizationRoleApiClient`; permissioned manual test pending. |
| TC-07 Department List and CRUD | PARTIAL | Live CRUD paths present; manual CRUD flow pending. Department task CRUD now guarded. |
| TC-08 Event List | PARTIAL | Live list path and status-label fix implemented (`Planning -> PLANNING`); manual UI verification pending. |
| TC-09 Event Detail | PARTIAL | Live event detail path remains wired; manual page verification pending. |
| TC-10 Milestone Flow | PARTIAL | Live milestone service path retained; manual create/update/delete verification pending. |
| TC-11 Category Flow | PARTIAL | Live category service path retained; manual create/update/delete verification pending. |
| TC-12 Task Flow / Board | PARTIAL | Status normalization and legal transition guard added; manual board interaction pending. |
| TC-13 Request Flow | PARTIAL | Existing live request client retained; manual permissioned review flow pending. |
| TC-14 Notification Flow | PARTIAL | Existing live notification client retained; manual badge/read actions pending. |
| TC-15 Unsupported Routes Hidden/Disabled | PARTIAL | Nav entries and direct page guards added; full route click-through still needs manual pass. |

## TC-01 — Login Success
- Preconditions: Backend/frontend running; seeded account exists.
- Account: `example1@gmail.com / example1`
- Steps: Open `/login`, submit credentials.
- Expected API calls: `POST /api/auth/login`
- Expected UI result: Auth success; redirect to `/user/organizations`.
- Expected DB effect if any: `User.LastLogin` update.
- Pass/Fail: PARTIAL
- Notes: API call verified via smoke script.

## TC-02 — Auth Me Bootstrap
- Preconditions: Token present from login.
- Account: same
- Steps: Refresh app after login.
- Expected API calls: `GET /api/auth/me`
- Expected UI result: User remains authenticated.
- Expected DB effect if any: none
- Pass/Fail: PARTIAL
- Notes: Endpoint verified; browser refresh behavior pending manual.

## TC-03 — Default Organization Context
- Preconditions: Authenticated user with membership.
- Account: same
- Steps: Open `/org/events` or `/org/members`.
- Expected API calls: `GET /api/organizations/default`
- Expected UI result: Org id resolves; no context crash.
- Expected DB effect if any: none
- Pass/Fail: PARTIAL
- Notes: Endpoint response verified (`data.id` returned).

## TC-04 — Organization Overview
- Preconditions: Valid org id.
- Account: same
- Steps: Open organization overview page.
- Expected API calls: `/public-overview`, `/permissions/me`, `/events`, `/departments`, `/members`
- Expected UI result: Basic info renders; missing mock-only fields are null-guarded.
- Expected DB effect if any: none
- Pass/Fail: PARTIAL
- Notes: FE enrichment implemented; manual rendering pass pending.

## TC-05 — Member List Load
- Preconditions: Access to member page.
- Account: same
- Steps: Open `/org/members`.
- Expected API calls: members, departments, roles, permissions.
- Expected UI result: Members and role/department context load.
- Expected DB effect if any: none
- Pass/Fail: PARTIAL
- Notes: Self-protection fallback changed to email when `UserId` is not provided.

## TC-06 — Role Assignment
- Preconditions: Role-management permission.
- Account: manager/president test account
- Steps: Assign new role in member page.
- Expected API calls: `POST /api/organizations/{id}/members/{memberId}/role` with `{ roleId }`
- Expected UI result: Success and refreshed member role.
- Expected DB effect if any: member role update
- Pass/Fail: PARTIAL
- Notes: Canonical roleId endpoint is wired; manual permission test pending.

## TC-07 — Department List and CRUD
- Preconditions: Department management permission.
- Account: manager-level account
- Steps: List/create/update/delete department.
- Expected API calls: list/create/update/delete department endpoints.
- Expected UI result: List updates after operations.
- Expected DB effect if any: department records changed
- Pass/Fail: PARTIAL
- Notes: Department task CRUD actions intentionally disabled in live demo path.

## TC-08 — Event List
- Preconditions: Valid org context.
- Account: same
- Steps: Open `/org/events`.
- Expected API calls: `GET /api/organizations/{orgId}/events`
- Expected UI result: Event list renders and status labels are correct.
- Expected DB effect if any: none
- Pass/Fail: PARTIAL
- Notes: `Planning` mapping fixed to `PLANNING`.

## TC-09 — Event Detail
- Preconditions: Existing event id.
- Account: same
- Steps: Open `/org/events/{eventId}`.
- Expected API calls: event detail + milestones.
- Expected UI result: Detail and planning sections render.
- Expected DB effect if any: none
- Pass/Fail: PARTIAL
- Notes: Public registration action disabled to avoid unsupported endpoint path.

## TC-10 — Milestone Flow
- Preconditions: Event management permission.
- Account: manager-level account
- Steps: Create/edit/delete milestone.
- Expected API calls: milestone CRUD endpoints.
- Expected UI result: Milestone list reflects updates.
- Expected DB effect if any: milestone rows changed
- Pass/Fail: PARTIAL
- Notes: Manual mutation test pending.

## TC-11 — Category Flow
- Preconditions: Existing milestone.
- Account: manager-level account
- Steps: Create/edit/delete category.
- Expected API calls: category endpoints.
- Expected UI result: Category list updates.
- Expected DB effect if any: category rows changed
- Pass/Fail: PARTIAL
- Notes: Manual mutation test pending.

## TC-12 — Task Flow / Board
- Preconditions: Existing category.
- Account: manager-level account
- Steps: Create task and update status.
- Expected API calls: task list/create/status endpoints.
- Expected UI result: Board updates and disallows illegal transitions.
- Expected DB effect if any: task rows changed
- Pass/Fail: PARTIAL
- Notes: Single-assignee guard added for live API compatibility.

## TC-13 — Request Flow
- Preconditions: Request permissions and proper roles.
- Account: submitter + reviewer
- Steps: Submit and review request.
- Expected API calls: request submit/list/review endpoints.
- Expected UI result: Request state changes correctly.
- Expected DB effect if any: request status update
- Pass/Fail: PARTIAL
- Notes: Manual permissioned flow pending.

## TC-14 — Notification Flow
- Preconditions: User session.
- Account: same
- Steps: Open notifications and mark read/read-all.
- Expected API calls: list/unread/mark-read/read-all.
- Expected UI result: Badge count updates; empty list handled.
- Expected DB effect if any: notification read markers updated
- Pass/Fail: PARTIAL
- Notes: Empty response is valid.

## TC-15 — Unsupported Routes Hidden/Disabled
- Preconditions: Live mode enabled.
- Account: same
- Steps: Inspect nav + direct URL behavior for unsupported routes/actions.
- Expected API calls: none for hidden actions.
- Expected UI result: Unsupported features hidden or show safe "coming soon" state.
- Expected DB effect if any: none
- Pass/Fail: PARTIAL
- Notes: Nav hiding and direct route guards implemented for home/discover/friends/messages; final click-through pending.
