# Unsupported Features For Live Demo

- Updated in Phase 2B on 2026-05-06.
- Canonical contract source: `Docs/swagger-live.json` and `Docs/API_LIVE_CONTRACT_MATRIX.md`.
- Policy: unsupported features are hidden from nav and/or guarded with a safe state; no mock fallback is allowed in live mode.

## 1) Home Dashboard (`/home`)
- Why unsafe: `OverviewApiClient` throws `NotSupportedException`.
- Evidence: `src/Org.Frontend/Services/Overview/OverviewApiClient.cs`.
- Temporary behavior: direct route shows live-unavailable state.
- Implemented in Phase 2B:
  - `src/Org.Frontend/Components/Pages/Home.razor`.

## 2) Messages (`/user/messages*`)
- Why unsafe: `MessageApiClient` methods throw `NotSupportedException`.
- Evidence: `src/Org.Frontend/Services/Messages/MessageApiClient.cs`.
- Temporary behavior: hidden from navigation + direct route guarded.
- Implemented in Phase 2B:
  - `src/Org.Frontend/Components/Layout/NavMenu.razor`
  - `src/Org.Frontend/Components/Layout/MainLayout.razor`
  - `src/Org.Frontend/Components/Pages/User/Messages.razor`

## 3) Discover (`/user/discover`)
- Why unsafe: `DiscoverApiClient` throws `NotSupportedException`.
- Evidence: `src/Org.Frontend/Services/Discover/DiscoverApiClient.cs`.
- Temporary behavior: hidden from navigation + direct route guarded.
- Implemented in Phase 2B:
  - `src/Org.Frontend/Components/Layout/NavMenu.razor`
  - `src/Org.Frontend/Components/Pages/User/Discover.razor`

## 4) Friends (`/user/friends`)
- Why unsafe: `FriendApiClient` throws `NotSupportedException`.
- Evidence: `src/Org.Frontend/Services/Friends/FriendApiClient.cs`.
- Temporary behavior: hidden from navigation + direct route guarded.
- Implemented in Phase 2B:
  - `src/Org.Frontend/Components/Layout/NavMenu.razor`
  - `src/Org.Frontend/Components/Pages/User/Friends.razor`

## 5) Posts Feed/Composer
- Why unsafe: `IPostService` is still bound to `PostMockService` in DI.
- Evidence: `src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs`.
- Temporary behavior: keep out of live demo script.
- Recommended Phase 2C action: implement `PostApiClient` + live DI wiring.

## 6) Event Registration/Unregistration
- Why unsafe: backend registration endpoints are not present in current Swagger contract.
- Evidence:
  - `Docs/swagger-live.json` (no registration endpoint)
  - `src/Org.Frontend/Services/Events/EventApiClient.cs` (register/unregister throws)
- Temporary behavior: disable registration action in public event page.
- Implemented in Phase 2B:
  - `src/Org.Frontend/Components/Pages/Events/PublicEventDetail.razor`

## 7) Department Task CRUD Panel
- Why unsafe: department-task live methods throw `NotSupportedException`.
- Evidence: `src/Org.Frontend/Services/Departments/DepartmentApiClient.cs`.
- Temporary behavior: keep department task controls unreachable in live demo path.
- Implemented in Phase 2B:
  - `src/Org.Frontend/Components/Pages/Departments/DepartmentList.razor`

## 8) Organization Resources (`/org/resources`)
- Why unsafe: no validated live service contract in core demo path.
- Temporary behavior: hide route from main org nav and keep out of demo script.
- Implemented in Phase 2B:
  - `src/Org.Frontend/Components/Layout/NavMenu.razor`

## 9) Organization Finance (`/org/finance`)
- Why unsafe: no validated live service contract in core demo path.
- Temporary behavior: hide route from main org nav and keep out of demo script.
- Implemented in Phase 2B:
  - `src/Org.Frontend/Components/Layout/NavMenu.razor`

## 10) Organization Reports (`/org/reports`)
- Why unsafe: no validated live service contract in core demo path.
- Temporary behavior: hide route from main org nav and keep out of demo script.
- Implemented in Phase 2B:
  - `src/Org.Frontend/Components/Layout/NavMenu.razor`

## 11) User Settings Partial Actions
- Why unsafe: several operations are marked unsupported by `UserSettingsApiClient`.
- Evidence: `src/Org.Frontend/Services/UserSettings/UserSettingsApiClient.cs`.
- Temporary behavior: keep unsupported actions disabled/clear in UI.
- Note: post-action navigation now returns to live-safe route (`/user/organizations`).

## 12) Backend Admin Migration Endpoint
- Why unsafe: administrative endpoint must not be exposed in frontend demo.
- Evidence: backend endpoint exists (`/api/admin/apply-migration`).
- Temporary behavior: no frontend route/action for this endpoint.

## Live-Safe Core Demo Modules (after Phase 2B wiring)
- Auth/login/bootstrap
- Organization context/default organization
- Organization overview (with live enrichment + null guards)
- Members + role assignment via organization `roleId` endpoint
- Departments core list/CRUD (excluding department-task CRUD)
- Events list/detail (excluding registration)
- Milestones/categories/tasks core flows
- Requests + notifications (existing live clients; still needs manual role-based verification)
