# PHASE2C_AUTHFIX Report

Date: 2026-05-06
Scope: Blazor Server auth token bridging / server-side HttpClient Authorization attachment.

## 1) Was CircuitServicesAccessorHandler registered?
- Before fix: **PARTIAL / incorrect bridging lifetime**
  - `CircuitServicesAccessor` was scoped, so handler/circuit contexts could resolve different accessor instances.
- After fix: **YES**
  - `services.AddSingleton<CircuitServicesAccessor>();`
  - `services.AddScoped<CircuitHandler, CircuitServicesAccessorHandler>();`
- File: `src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs`

## 2) Was protected page API loading happening before auth initialization?
- Before fix: **YES**
  - Core pages (`/user/organizations`, `/user/events`, `/org/events`, `/user/settings`) loaded data in `OnInitializedAsync`, which can execute before token state is initialized from browser storage in interactive flow.
- After fix: **NO (for the patched core pages)**
  - Core page loads moved to `OnAfterRenderAsync(firstRender)` and gated by:
    1. `FrontendAuthStateProvider.InitializeAsync()` completion
    2. authenticated identity check before calling API

## 3) Final token source behavior in AuthHeaderDelegatingHandler
- Added source-aware resolution:
  - Prefer circuit-scoped services via `CircuitServicesAccessor` (`tokenSource=circuit-scope`)
  - Fallback to handler scope (`tokenSource=handler-scope`)
- Added non-sensitive diagnostics:
  - request method/path
  - tokenSource
  - hasToken
  - expiresAtUtc
  - authHeaderAttached
- File: `src/Org.Frontend/Services/Auth/AuthHeaderDelegatingHandler.cs`

## 4) Confirmed server log evidence
### Prior confirmed failing evidence
- `AuthHeaderDelegatingHandler request GET /api/users/me/events; tokenSource=handler-scope; hasToken=False; authHeaderAttached=False`
- backend returned 401.

### Post-fix no-login smoke (server-side)
- Direct non-interactive route probes (`Invoke-WebRequest`) did not emit early backend API calls from patched core pages.
- This supports the timing fix (pages no longer eagerly call live API before auth initialization).

### Real-browser-login evidence
- **NEEDS_VERIFICATION in your local browser run** (this environment cannot perform full interactive browser login flow).
- Expected post-login log pattern for core calls:
  - `tokenSource=circuit-scope` (or equivalent valid source)
  - `hasToken=True`
  - `authHeaderAttached=True`

## 5) Root cause
Two combined issues:
1. **DI/lifetime bridging issue** for `CircuitServicesAccessor` reduced reliability of resolving circuit token services from the auth handler.
2. **Auth initialization timing**: several protected pages issued API calls before auth/token initialization completed, producing server-side calls without Authorization header.

## 6) Files changed
1. `src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs`
2. `src/Org.Frontend/Infrastructure/Auth/CircuitServicesAccessor.cs`
3. `src/Org.Frontend/Services/Auth/AuthHeaderDelegatingHandler.cs`
4. `src/Org.Frontend/Services/Auth/FrontendAuthStateProvider.cs`
5. `src/Org.Frontend/Components/Auth/RedirectToLogin.razor`
6. `src/Org.Frontend/Components/Layout/MainLayout.razor`
7. `src/Org.Frontend/Components/Pages/User/Organizations.razor`
8. `src/Org.Frontend/Components/Pages/User/Events.razor`
9. `src/Org.Frontend/Components/Pages/Events/EventList.razor`
10. `src/Org.Frontend/Components/Pages/Members/MemberList.razor`
11. `src/Org.Frontend/Components/Pages/Departments/DepartmentList.razor`
12. `src/Org.Frontend/Components/Pages/User/Settings.razor`
13. `src/Org.Frontend/Components/Pages/Auth/Login.razor`

## 7) Build result
- Command: `dotnet build StudentOrgManager.slnx --no-restore`
- Result: **PASS** (`0` errors)

## 8) Verification result for required routes
Because interactive browser actions are required, current status is:

| Route | Status | Notes |
|---|---|---|
| `/user/organizations` | NEEDS_VERIFICATION | Timing guard applied; requires local browser login log confirmation. |
| `/org/events` | NEEDS_VERIFICATION | Timing guard applied; requires local browser login log confirmation. |
| `/org/members` | NEEDS_VERIFICATION | Auth init call added in page init; requires local browser login log confirmation. |
| `/org/departments` | NEEDS_VERIFICATION | Auth init call added in page init; requires local browser login log confirmation. |
| `/user/settings` | NEEDS_VERIFICATION | Timing guard + 401 signout behavior; requires local browser login log confirmation. |

## 9) Required local verification checklist (after this patch)
1. Restart frontend.
2. Clear `localhost:5236` site data.
3. Login `example1@gmail.com / example1`.
4. Open `/user/organizations`, `/org/events`, `/org/members`, `/org/departments`, `/user/settings`.
5. Confirm frontend server logs show for backend API calls:
   - `hasToken=True`
   - `authHeaderAttached=True`
   - token source should be circuit-resolved path when available.
6. Confirm backend responses are 200 and no immediate 401/session-expired message.
