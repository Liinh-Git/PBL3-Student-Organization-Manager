# PHASE2C_AUTHFIX Report

Date: 2026-05-06
Task: PHASE2C-AUTHFINAL

## 1) Why DelegatingHandler + Circuit Bridge Was Unreliable

Root issue in Blazor Server runtime:
- Core API calls were depending on `AuthHeaderDelegatingHandler` resolving token state via `CircuitServicesAccessor`.
- In real flow, some requests ran with circuit-scoped token state (worked), but others ran with fallback handler scope (missing token), causing inconsistent `Authorization` attachment.
- Observed pattern: same session had both success calls (`hasToken=true`, `authHeaderAttached=true`) and failed calls (`hasToken=false`, `authHeaderAttached=false`, 401).

Conclusion:
- DelegatingHandler + circuit bridge was not stable enough as the primary auth path for core protected API calls.

## 2) New Design: Scoped AuthenticatedBackendClient

Implemented:
- `src/Org.Frontend/Services/Auth/IAuthenticatedBackendClient.cs`
- `src/Org.Frontend/Services/Auth/AuthenticatedBackendClient.cs`

Design summary:
- Scoped helper used by core ApiClients.
- Uses named HttpClient `BackendApi`.
- Reads token directly from circuit-scoped `IAccessTokenStore` (no browser localStorage and no JS interop).
- Before every protected request:
  - if token missing/expired => throws `AuthApiException("AUTH_NOT_READY", 0)` and does not send request.
  - if token valid => attaches `Authorization: Bearer <token>` and sends.
- Response handling:
  - 401 => sign out via `FrontendAuthStateProvider`, navigate `/login`, throw `AuthApiException(..., 401)`.
  - 403 => keep token, throw permission `AuthApiException(..., 403)`.
  - other non-success => throw `AuthApiException` with safe parsed message + status.
- Logging added for method/path/token-state/attachment/status (token value never logged).

## 3) DI Changes

File:
- `src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs`

Changes:
- Added named client:
  - `services.AddHttpClient("BackendApi", ...)`
- Added scoped helper:
  - `services.AddScoped<IAuthenticatedBackendClient, AuthenticatedBackendClient>()`
- Core migrated clients switched from typed `AddHttpClient(...).AddHttpMessageHandler<AuthHeaderDelegatingHandler>()` to scoped registrations.
- Non-core/non-migrated clients may still temporarily use delegating handler path.

## 4) ApiClient Migration Status

### Migrated
1. `UserDashboardApiClient`
2. `OrganizationApiClient`
3. `OrganizationServiceApiClient`
4. `OrganizationRoleApiClient`
5. `EventApiClient`
6. `MemberApiClient`
7. `DepartmentApiClient`
8. `UserSettingsApiClient`
9. `NotificationService`
10. `MilestoneApiClient`
11. `EventCategoryApiClient`
12. `TaskApiClient`
13. `RequestApiClient`

### Not Migrated (within requested list)
- None.

## 5) Core Path Dependency on AuthHeaderDelegatingHandler

Core path no longer depends on `AuthHeaderDelegatingHandler` for migrated clients: **YES**.

Notes:
- Handler remains registered for non-core clients that were not part of this final migration scope.
- For migrated core clients, token attachment is now done by `AuthenticatedBackendClient`.

## 6) ApiClient Matrix

| ApiClient | Before auth method | After auth method | Migrated? | Notes |
|---|---|---|---|---|
| UserDashboardApiClient | HttpClient + DelegatingHandler | IAuthenticatedBackendClient | YES | `/api/users/me/*` now stable bearer path |
| OrganizationApiClient | Manual token read (store+localStorage fallback) | IAuthenticatedBackendClient | YES | Removed manual token-building logic |
| OrganizationServiceApiClient | HttpClient + DelegatingHandler | IAuthenticatedBackendClient | YES | Includes my orgs + create org |
| OrganizationRoleApiClient | HttpClient + DelegatingHandler | IAuthenticatedBackendClient | YES | Role CRUD + assign role |
| EventApiClient | HttpClient + DelegatingHandler | IAuthenticatedBackendClient | YES | My events/org events/detail/create/update/delete |
| MemberApiClient | Manual token read (store+localStorage fallback) | IAuthenticatedBackendClient | YES | Member CRUD + permission checks |
| DepartmentApiClient | Manual token read (store+localStorage fallback) | IAuthenticatedBackendClient | YES | Department CRUD + members |
| UserSettingsApiClient | HttpClient + DelegatingHandler | IAuthenticatedBackendClient | YES | `/api/users/me` load/save profile |
| NotificationService | HttpClient + DelegatingHandler | IAuthenticatedBackendClient | YES | Notification API path migrated; SignalR unchanged |
| MilestoneApiClient | HttpClient + DelegatingHandler | IAuthenticatedBackendClient | YES | Milestone CRUD |
| EventCategoryApiClient | HttpClient + DelegatingHandler | IAuthenticatedBackendClient | YES | Category CRUD |
| TaskApiClient | HttpClient + DelegatingHandler | IAuthenticatedBackendClient | YES | Task list/create/status/update permission checks |
| RequestApiClient | HttpClient + DelegatingHandler | IAuthenticatedBackendClient | YES | Request submit/review/list/detail |

## 7) Build Result

Command:
- `dotnet build StudentOrgManager.slnx --no-restore`

Result:
- **PASS**
- 0 errors
- 10 existing MudBlazor analyzer warnings (unrelated to auth migration)

## 8) Manual Verification Checklist (Required)

1. Restart Backend and Frontend.
2. Clear site data for `localhost:5236`.
3. Login with `example1@gmail.com / example1`.
4. Open directly (no Retry):
   - `/user/organizations`
   - `/user/events`
   - `/org/events`
   - `/org/members`
   - `/org/departments`
   - `/user/settings`
5. Create organization from `/user/organizations`.

Expected:
- brief loading only
- data auto-loads
- no mandatory Retry/reload
- no user-facing `AUTH_NOT_READY`
- no immediate `Phi�n dang nh?p d� h?t h?n` after fresh login
- 403 (if occurs) treated as permission issue, not session-expired flow

## 9) Remaining Blockers / Risks

- Runtime verification is still required to confirm no residual page-level edge case remains in actual login/navigation timing.
- Some non-core services still keep delegating-handler path temporarily; they are outside this final core migration requirement.

## 10) Boot/Render Blocker Fix (May 6, 2026)

Root cause:
- `MainLayout` gated rendering on `_authReady`, but `_authReady` could remain `false` forever when auth init failed or short-circuited (JS interop not ready). This blocked public routes like `/login`.

Fix summary:
- Public routes now bypass the auth-ready gate and render immediately.
- Auth init uses `try/catch/finally`; `_authReady` is always set in `finally` and `StateHasChanged` is invoked.
- Redirect to `/login` happens only for protected routes when auth is missing or init fails.
- Added safe diagnostics (route, isPublicRoute, init start/success/fail, authReady set).

Files changed:
- `src/Org.Frontend/Components/Layout/MainLayout.razor`

Build result:
- `dotnet build StudentOrgManager.slnx --no-restore`
- **FAILED** (Org.Backend locked by running process). Frontend still compiled; errors were file-lock related.

Manual test checklist:
1. Restart frontend.
2. Clear site data for `localhost:5236`.
3. Open `/login` (should render immediately, no infinite spinner).
4. Login, then open:
  - `/user/organizations`
  - `/user/events`
  - `/org/events`
  - `/org/members`
  - `/org/departments`
5. Expect brief loading only; no permanent spinner.
