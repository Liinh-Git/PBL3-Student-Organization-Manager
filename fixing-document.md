# 1. Root cause

**Core root cause:** the repo has no single enforced contract from **Database → EF Entity → DTO → API endpoint → Frontend API client → Adapter/ViewModel → UI**. Backend, frontend, shared DTOs, and mock data evolved separately.

**Why BE/FE end-to-end fails reliably:**

* **Frontend is currently protected by mock mode.** `src/Org.Frontend/appsettings.json` has `FrontendData.UseMockServices = true`, so many screens can render even when the real Backend contract is wrong or missing.
* **There are two DTO systems:**

  * `src/Org.Shared/Features/...` = actual Backend API contracts.
  * `src/Org.Shared/Contracts/...` = legacy/mock/internal Frontend contracts.
    This forces lossy adapters in Frontend API clients.
* **Mock data shape does not match real API shape.** Example: mock organizations use `orgName`, while real API uses `Name` through `OrganizationDto`.
* **Several Frontend live clients call modules that Backend does not actually expose yet**, or they throw `NotSupportedException` in live mode.
* **Role/member mapping is broken.** Backend roles are DB entities and `MemberRole` enum names, while Frontend has hardcoded role GUIDs that are not canonical.
* **Event DTO is too small compared to Entity/UI needs.** Entity has `Location`, `Budget`, `TargetParticipants`, `AverageRating`, but API `EventDto` omits several of these.
* **Seeder/demo data exists, but some seeded values are inconsistent**, especially role permissions and organization member counts.

The class diagram shows the intended domain: User, Organization/Club, Department, Event, Milestone, Task, Member, Role, Permission, Request, Resource, Notification, etc. But it is only low-trust design intent, not implementation truth. It also uses mixed naming like `Club`/`Organization`, `orgId`/`clubId`, and a simplified Event → Milestone → Task chain that differs from the current EF model. 

I inspected the uploaded ZIP repo, not a live database. The GitHub branch reference provided points to `large-scale-refactor`. 

**Not verified in this environment:** actual `dotnet build`, runtime startup, EF migration execution, and live DB schema, because the container does not have the .NET SDK/runtime available. The included `build_log.txt` shows a Windows build failure caused by a locked `Org.Shared.dll` from a running process, not necessarily a compile error.

---

# 2. Repo architecture map

| Area                       | Path                                                                                                        |                                         Purpose | Trust level | Reason                                                       |
| -------------------------- | ----------------------------------------------------------------------------------------------------------- | ----------------------------------------------: | ----------: | ------------------------------------------------------------ |
| Solution                   | `StudentOrgManager.slnx`                                                                                    |                                   Main solution |        High | Defines actual projects                                      |
| Backend project            | `src/Org.Backend`                                                                                           |     API, EF Core, services, seed, FastEndpoints |        High | Current Backend implementation                               |
| Frontend project           | `src/Org.Frontend`                                                                                          | Blazor Server UI, API clients, mock/live switch |      Medium | UI works partly because mock mode hides live issues          |
| Shared contracts           | `src/Org.Shared/Features`                                                                                   |           Actual API request/response contracts |        High | Used by Backend endpoints                                    |
| Legacy/shared FE contracts | `src/Org.Shared/Contracts`                                                                                  |              Older DTOs used by mocks/FE models |  Low/Medium | Not canonical for Backend                                    |
| EF DbContext               | `src/Org.Backend/Infrastructure/Database/AppDbContext.cs`                                                   |                          DB model configuration |        High | Closest source of DB truth in repo                           |
| EF entities                | `src/Org.Backend/Domain/Entities`                                                                           |                     Canonical domain/data model |        High | Source of table/relationship intent                          |
| Migrations                 | `src/Org.Backend/Migrations`                                                                                |                     Schema evolution / snapshot |        High | Closest DB schema source without live DB                     |
| Seeder                     | `src/Org.Backend/Infrastructure/Database/DatabaseSeeder.cs`                                                 |                              Demo/dev seed data |      Medium | Useful but has consistency problems                          |
| Mock exporter              | `src/Org.Backend/Infrastructure/Database/MockDataExporter.cs`                                               |         Exports DB data into Frontend mock JSON |  Medium/Low | Exports legacy mock shape, not real API shape                |
| Frontend startup           | `src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs`                                      |                      Chooses mock/live services |        High | Directly controls E2E behavior                               |
| Frontend API clients       | `src/Org.Frontend/Services/ApiClients`                                                                      |                       Live API callers/adapters |      Medium | Some match Backend; some throw/not fully mapped              |
| Frontend mocks             | `src/Org.Frontend/Services/Mocks`                                                                           |                        Mock services/data store |         Low | Must not be canonical                                        |
| Mock data files            | `src/Org.Frontend/Services/Mocks/Data/*.mock.json`                                                          |                                  Demo JSON data |         Low | Shape differs from real API                                  |
| Auth/role logic            | Backend: `Features/Auth`, `Infrastructure/Auth`, `Shared/MemberRole`; Frontend: auth services/layout guards |              Login, role routing, authorization |      Medium | Auth mostly present, but role/permission mapping is unstable |
| Docs                       | `README.md`, `API_SUMMARY.md`, `DTO.md`, `Docs/*`                                                           |                                Design/API notes |  Low/Medium | Several docs are outdated vs source code                     |
| Tests                      | `tests/Org.Backend.*`                                                                                       |                          Unit/integration tests |      Medium | Useful, but not enough to prove E2E demo                     |

---

# 3. Database/Entity audit

## Canonical EF model

The closest available database truth is:

* `src/Org.Backend/Domain/Entities/*.cs`
* `src/Org.Backend/Infrastructure/Database/AppDbContext.cs`
* `src/Org.Backend/Migrations/AppDbContextModelSnapshot.cs`
* `src/Org.Backend/Migrations/*.cs`

The real DB schema was not included. Confidence: **medium-high from EF code**, but **needs verification against the actual dev DB**.

## Main entities/tables

| Entity           | Path                                | Key fields                                                                                                                                                    | Relationships                                                                          |      Seed exists | Mismatch risk                                                                      |
| ---------------- | ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- | ---------------: | ---------------------------------------------------------------------------------- |
| `User`           | `Domain/Entities/User.cs`           | `FullName`, `Email`, `PasswordHash`, `PhoneNumber`, `Dob`, `Gender`, `Address`, `AvatarUrl`, `Bio`, `SocialLinks`, `Status`, `ProfileVisibility`, `LastLogin` | Has many `Members`, `Requests`, `Notifications`, `Attendees`, `Ratings`                |              Yes | FE/Auth mostly okay, but some profile/discover features depend on extra mapping    |
| `Organization`   | `Domain/Entities/Organization.cs`   | `OrgName`, `Description`, `AvatarUrl`, `CoverUrl`, `FoundingDate`, `Location`, `TotalMembers`, `Status`                                                       | Has many `Departments`, `Members`, `Events`, `Roles`, `Resources`, `Requests`, `Posts` |              Yes | API renames `OrgName` → `Name`; mock uses `orgName`; FE must not assume mock shape |
| `Member`         | `Domain/Entities/Member.cs`         | `UserId`, `OrgId`, `DepartmentId?`, `RoleId?`, `JoinDate`                                                                                                     | Belongs to `User`, `Organization`, `Department`, `Role`                                |              Yes | API `MemberDto` omits `UserId` and `RoleId`; FE legacy model needs them            |
| `Role`           | `Domain/Entities/Role.cs`           | `RoleName`, `Description`, `OrgId`, `IsDefault`                                                                                                               | Has many `Members`, `RolePermissions`                                                  |              Yes | Backend authorization depends on role names matching `MemberRole` enum             |
| `Permission`     | `Domain/Entities/Permission.cs`     | `PermissionKey`, `DisplayName`, `ModuleGroup`                                                                                                                 | Many-to-many via `RolePermission`                                                      |              Yes | Seeder creates generic permissions, not canonical `org.*` permissions              |
| `RolePermission` | `Domain/Entities/RolePermission.cs` | Composite `RoleId`, `PermissionId`                                                                                                                            | Links `Role` and `Permission`                                                          |              Yes | Seeder assignment loop appears wrong                                               |
| `Department`     | `Domain/Entities/Department.cs`     | `OrgId`, `DeptName`, `Code?`, `ManagerId?`, `Function`                                                                                                        | Belongs to `Organization`; manager is `Member`                                         |              Yes | API renames `DeptName` → `Name`, `Function` → `Description`                        |
| `Event`          | `Domain/Entities/Event.cs`          | `OrgId`, `EventName`, `Description`, `StartDate`, `EndDate`, `Budget`, `Location`, `TargetParticipants`, `Tags`, `Status`, `Visibility`, `AverageRating`      | Has `Milestones`, `EventMembers`, `Attendees`, `DigitalAssets`, `Reports`, `Ratings`   |              Yes | API `EventDto` omits several fields UI/mock expect                                 |
| `Milestone`      | `Domain/Entities/Milestone.cs`      | `EventId`, `Title`, `Description`, `OrderIndex`, `StartDate`, `EndDate`, `Status`                                                                             | Belongs to `Event`; has categories                                                     |              Yes | API maps `Title` → `Name`, `OrderIndex` → `SortOrder`                              |
| `EventCategory`  | `Domain/Entities/EventCategory.cs`  | `MilestoneId`, `CategoryName`, `OrderIndex`, `Description`, `OwnerDepartmentId?`                                                                              | Belongs to `Milestone`; has tasks                                                      |              Yes | UI/mock has extra fields like `leadMemberId`, `isUrgent`, `guidelines`             |
| `OrgTask`        | `Domain/Entities/OrgTask.cs`        | `EventCategoryId`, `TaskName`, `AssigneeId?`, `DeptId?`, `Priority`, `Deadline`, `Status`, `Note`                                                             | Table name is `Tasks`; belongs to category, member, department                         |              Yes | Mock supports more flexible assignees than Backend                                 |
| `Request`        | `Domain/Entities/Request.cs`        | `SenderId`, `OrgId`, `RequestType`, `Content`, `RequestDate`, `Status`                                                                                        | Belongs to `User` and `Organization`                                                   |              Yes | API request DTO has many fields not present in entity                              |
| `Notification`   | `Domain/Entities/Notification.cs`   | `ReceiverId`, `Title`, `Message`, `Type`, `IsRead`, `ActorId`, `RelatedEntityId`, `ActionUrl`, `ReadAt`                                                       | Belongs to receiver user                                                               | Migration exists | Mostly aligned                                                                     |

## Important DB/seed issues

### Confirmed

1. **Soft delete exists globally**
   `BaseEntity` has `IsDeleted`; `AppDbContext` applies soft-delete filters.

2. **Enums are converted to integers in DB**
   Several entity status fields are configured with enum conversions.

3. **`OrgTask` maps to table `Tasks`**
   This matters when comparing diagrams/docs/mock names.

4. **`Member` has unique `UserId + OrgId`**
   One user cannot be duplicated in the same organization.

5. **Seeded login accounts exist**
   Seeder creates accounts like:

   * `example1@gmail.com` / `example1`
   * `example2@gmail.com` / `example2`
   * etc.

6. **Seed data is enough for organization/event/member demo**, but not necessarily for every UI feature.

### Suspected / needs verification

1. **Seeder role-permission assignment bug**
   The seeder creates 3 roles per organization: `President`, `Manager`, `Member`, but later appears to index roles as if there are 2 roles per organization. This can assign permissions to wrong roles.

2. **`TotalMembers` may be stale**
   Organizations are seeded with `TotalMembers`, but actual seeded `Members` count may not match.

3. **`VicePresident` role is in enum but may not be seeded**
   Backend permission logic references role levels including `VicePresident`, but seeder seems to create only `President`, `Manager`, `Member`.

4. **Real DB may not match migrations**
   Need actual command output:

   ```powershell
   dotnet ef database update --project src\Org.Backend\Org.Backend.csproj
   dotnet run --project src\Org.Backend\Org.Backend.csproj -- --seed
   ```

---

# 4. Backend contract audit

## Actual endpoint style

Backend uses FastEndpoints. Response shapes are inconsistent but mostly intentional:

| Pattern                 | Example                                                      |
| ----------------------- | ------------------------------------------------------------ |
| List endpoints          | wrapper with `Items`, `TotalCount`, maybe `Page`, `PageSize` |
| Detail endpoints        | wrapper with `Data`                                          |
| Create/update endpoints | usually raw DTO                                              |
| Delete endpoints        | `204 No Content`                                             |

This is not automatically wrong, but it becomes dangerous when Frontend assumes one universal envelope.

## Backend contract table

| Module                        | Entity/Table                              | Request DTO                                              | Response DTO                                              | Controller/Endpoint                                        | Service/UseCase       | Status                | Notes                                                      |
| ----------------------------- | ----------------------------------------- | -------------------------------------------------------- | --------------------------------------------------------- | ---------------------------------------------------------- | --------------------- | --------------------- | ---------------------------------------------------------- |
| Auth login                    | `User`                                    | `LoginRequest`                                           | `LoginResponse`                                           | `POST /api/auth/login`                                     | Auth feature          | OK                    | Response is flat: token + user fields, not `{ user: ... }` |
| Auth me                       | `User`                                    | none                                                     | `MeResponse`                                              | `GET /api/auth/me`                                         | Auth feature          | OK                    | Requires token                                             |
| Organizations list            | `Organization`                            | query                                                    | `GetOrganizationsResponse`                                | `GET /api/organizations`                                   | Organizations feature | OK                    | Uses `Items` wrapper                                       |
| Organization detail           | `Organization`                            | route id                                                 | `GetOrganizationByIdResponse`                             | `GET /api/organizations/{id}`                              | Organizations feature | OK                    | Uses `Data` wrapper                                        |
| Default organization          | `Organization`/`Member`                   | none                                                     | `GetDefaultOrganizationResponse`                          | `GET /api/organizations/default`                           | Organizations feature | OK but fragile        | Fails if user has no membership                            |
| Organization create/update    | `Organization`                            | `CreateOrganizationRequest`, `UpdateOrganizationRequest` | raw `OrganizationDto`                                     | `POST/PUT /api/organizations`                              | Organizations feature | OK                    | Raw response, not envelope                                 |
| Public overview               | `Organization`                            | route id                                                 | `GetPublicOrganizationOverviewResponse`                   | `GET /api/organizations/{id}/public-overview`              | Org management        | OK                    | FE uses this for overview                                  |
| Permissions me                | `Member`/`Role`/`Permission`              | route id                                                 | `GetOrganizationPermissionsMeResponse`                    | `GET /api/organizations/{id}/permissions/me`               | Org management        | OK but role-sensitive | Depends on seeded roles/permissions                        |
| Roles                         | `Role`                                    | role DTOs                                                | `GetOrganizationRolesResponse`, raw `OrganizationRoleDto` | `/api/organizations/{id}/roles`                            | Org management        | MISMATCH risk         | FE member role assignment uses hardcoded GUIDs elsewhere   |
| Members list                  | `Member`                                  | query                                                    | `GetMembersResponse`                                      | `GET /api/organizations/{orgId}/members`                   | Members feature       | MISMATCH              | `MemberDto` lacks `UserId`, `RoleId` needed by legacy FE   |
| Member role/department update | `Member`                                  | update requests                                          | raw `MemberDto`                                           | `PUT /api/members/{id}/role`, `/department`                | Members feature       | MISMATCH              | Uses enum role update, but FE may pass hardcoded role GUID |
| Departments list              | `Department`                              | query                                                    | `GetDepartmentsResponse`                                  | `GET /api/organizations/{orgId}/departments`               | Departments feature   | OK                    | Mapping renames fields                                     |
| Department detail             | `Department`                              | route id                                                 | `GetDepartmentByIdResponse`                               | `GET /api/departments/{id}`                                | Departments feature   | OK                    | Uses `Data` wrapper                                        |
| Department CRUD               | `Department`                              | create/update DTOs                                       | raw `DepartmentDto`                                       | `POST /api/departments`, `PUT /api/departments/{id}`       | Departments feature   | OK                    | Good enough for demo                                       |
| Department tasks overview     | `OrgTask`                                 | query                                                    | `GetDepartmentTasksOverviewResponse`                      | `GET /api/departments/{id}/tasks/overview`                 | Departments feature   | OK                    | Read-only overview only                                    |
| Department task CRUD          | none dedicated                            | legacy FE request                                        | none                                                      | none                                                       | none                  | MISSING               | FE live methods throw                                      |
| Event list                    | `Event`                                   | query                                                    | `GetOrganizationEventsResponse`                           | `GET /api/organizations/{orgId}/events`                    | Events feature        | PARTIAL               | API omits `Location`, `Budget`, `TargetParticipants`       |
| Event detail                  | `Event`                                   | route id                                                 | `GetEventByIdResponse`                                    | `GET /api/events/{id}`                                     | Events feature        | PARTIAL               | Same DTO limitation                                        |
| Event create/update           | `Event`                                   | create/update DTOs                                       | raw `EventDto`                                            | `POST /api/events`, `PUT /api/events/{id}`                 | Events feature        | PARTIAL               | Entity has more fields than DTO                            |
| Public events                 | `Event`                                   | query                                                    | `GetPublicEventsResponse`                                 | `GET /api/events/public`                                   | Events feature        | PARTIAL               | Good route, small DTO                                      |
| Event registration            | `Attendee` or `EventMember`               | none                                                     | none                                                      | none                                                       | none                  | MISSING               | FE live register/unregister throws                         |
| Milestones                    | `Milestone`                               | milestone DTOs                                           | wrappers/raw DTO                                          | `/api/events/{eventId}/milestones`, `/api/milestones/{id}` | Milestones feature    | OK                    | Good enough                                                |
| Categories                    | `EventCategory`                           | category DTOs                                            | wrappers/raw DTO                                          | `/api/milestones/{id}/categories`, `/api/categories/{id}`  | Categories feature    | OK                    | Some derived fields only                                   |
| Tasks                         | `OrgTask`                                 | task DTOs                                                | wrappers/raw DTO                                          | `/api/categories/{id}/tasks`, `/api/tasks/{id}`            | Tasks feature         | OK/PARTIAL            | Single assignee only                                       |
| Requests                      | `Request`                                 | request DTOs                                             | request response DTO                                      | `/api/organizations/{id}/requests`                         | Requests feature      | MISMATCH              | DTO has fields not stored separately in entity             |
| Notifications                 | `Notification`                            | notification DTOs                                        | wrappers/raw action DTOs                                  | `/api/notifications`                                       | Notifications feature | OK                    | Mostly aligned                                             |
| Posts                         | `OrganizationPost`                        | backend exists partly                                    | unknown from FE                                           | backend routes may exist                                   | Posts feature         | DUPLICATED/UNSTABLE   | FE always uses mock service                                |
| Messages                      | none confirmed                            | FE expects                                               | none                                                      | none                                                       | none                  | MISSING               | Live client throws                                         |
| Finance/Reports/Resources UI  | `Resource` exists, reports entities exist | FE expects richer module                                 | no complete API                                           | none/partial                                               | MISSING               | Exclude from demo     |                                                            |

---

# 5. Frontend mapping audit

## Frontend live/mock switch

Critical file:

```text
src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs
```

Current behavior:

* Mock mode is enabled by default from:

```text
src/Org.Frontend/appsettings.json
FrontendData.UseMockServices = true
```

* Live mode registers API clients, but some live clients intentionally throw.
* `IPostService` always resolves to `PostMockService`, even in live mode.

This is the biggest reason UI rendering does not prove Backend/Frontend integration.

## Frontend mapping table

| Module                | API Service                    | FE-called endpoint                                                | FE-assumed response shape                   | Adapter/Mapper                | UI risk                                         | Mock data               | Status         | Notes                                       |
| --------------------- | ------------------------------ | ----------------------------------------------------------------- | ------------------------------------------- | ----------------------------- | ----------------------------------------------- | ----------------------- | -------------- | ------------------------------------------- |
| Auth                  | `AuthApiClient`                | `/api/auth/login`, `/api/auth/register`, `/api/auth/me`           | Actual shared auth contracts                | Minimal                       | Login may work                                  | Not main issue          | OK             | Backend response is flat, not old doc shape |
| Current organization  | `OrganizationApiClient`        | `/api/organizations/default`                                      | `GetDefaultOrganizationResponse.Data.Id`    | Caches org id                 | Fails if user has no org                        | Mock has orgs           | OK but fragile | Needs seeded member user                    |
| Organization overview | `OrganizationServiceApiClient` | `/permissions/me`, `/public-overview`, `/events`                  | Mixed wrappers                              | Builds overview VM            | Missing departments/timeline/leadership live    | Mock richer             | PARTIAL        | Good enough if overview is simple           |
| Organization CRUD     | `OrganizationServiceApiClient` | `/api/organizations`                                              | list/detail wrappers, create raw DTO        | Maps `Name`/`Description`     | Mostly okay                                     | Mock uses `orgName`     | OK/PARTIAL     | Must not use mock shape as API              |
| Roles                 | `OrganizationRoleApiClient`    | `/roles`, `/members/{memberId}/role`                              | Role wrappers/raw DTO                       | Maps permissions              | Role assignment conflict with `MemberApiClient` | Mock role IDs differ    | MISMATCH       | Use role API or enum, not hardcoded GUIDs   |
| Members               | `MemberApiClient`              | `/api/organizations/{orgId}/members`, `/api/members/{id}/role`    | `GetMembersResponse.Items`, raw `MemberDto` | Feature DTO → legacy contract | `UserId = Guid.Empty`; role IDs fake            | Mock has user/role ids  | MISMATCH       | P0/P1 depending member admin demo           |
| Departments           | `DepartmentApiClient`          | `/api/organizations/{orgId}/departments`, `/api/departments/{id}` | Actual feature wrappers/raw DTO             | Feature DTO → legacy contract | CRUD mostly okay                                | Mock uses legacy fields | OK/PARTIAL     | Department task CRUD unsupported live       |
| Events                | `EventApiClient`               | `/api/organizations/{orgId}/events`, `/api/events/{id}`           | Actual event wrappers/raw DTO               | EventDto → UI event model     | Location/budget/slots missing or fake           | Mock richer             | PARTIAL        | Add minimal event fields or simplify UI     |
| Event registration    | `EventApiClient`               | expected register/unregister                                      | none                                        | throws                        | Button crashes if used                          | Mock may work           | MISSING        | Hide/disable for demo                       |
| Milestones            | `MilestoneApiClient`           | `/events/{id}/milestones`, `/milestones/{id}`                     | Actual wrappers/raw DTO                     | Good enough                   | May drop description/status on update           | Mock similar            | OK/PARTIAL     | Not P0                                      |
| Categories            | `EventCategoryApiClient`       | `/milestones/{id}/categories`, `/categories/{id}`                 | Actual wrappers/raw DTO                     | Good enough                   | Extra UI fields absent                          | Mock richer             | OK/PARTIAL     | Not P0                                      |
| Tasks                 | `TaskApiClient`                | `/categories/{id}/tasks`, `/tasks/{id}`                           | Actual wrappers/raw DTO                     | Single assignee mapping       | Multi-assignee UI breaks live                   | Mock richer             | PARTIAL        | Restrict to one assignee                    |
| User dashboard        | `UserDashboardApiClient`       | `/users/me/organizations`, `/users/me/events`, discover endpoints | Actual wrappers                             | Maps to cards                 | Mostly okay                                     | Mock richer             | OK/PARTIAL     | Check with seeded user                      |
| Discover              | `DiscoverApiClient`            | none implemented live                                             | none                                        | throws                        | Discover page breaks live                       | Mock only               | MISSING        | Hide or implement minimal mapping           |
| Overview              | `OverviewApiClient`            | none implemented live                                             | none                                        | throws                        | Overview page may break live                    | Mock only               | MISSING        | Do not demo unless fixed                    |
| Messages              | `MessageApiClient`             | none                                                              | none                                        | throws                        | Messages page breaks live                       | Mock only               | MISSING        | Exclude from demo                           |
| Posts                 | `PostMockService`              | mock only                                                         | mock shape                                  | mock store                    | Not real E2E                                    | Mock only               | MISMATCH       | Do not claim live Backend support           |
| Notifications         | `NotificationService`          | `/api/notifications`                                              | Actual notification wrappers                | Good enough                   | Token/SignalR dependent                         | Mock possibly exists    | OK             | Not first priority                          |

---

# 6. Mock data audit

Mock data location:

```text
src/Org.Frontend/Services/Mocks/Data/*.mock.json
src/Org.Frontend/Services/Mocks/Models/*.cs
src/Org.Frontend/Services/Mocks/FrontendMockDataStore.cs
```

Mock exporter:

```text
src/Org.Backend/Infrastructure/Database/MockDataExporter.cs
```

The mock exporter does not export real API DTO shapes. It exports legacy FE mock shapes.

| Mock File                      | Module           | Current Field/Shape                                              | Correct Contract Shape                                                             | Issue                                               | Severity                  |
| ------------------------------ | ---------------- | ---------------------------------------------------------------- | ---------------------------------------------------------------------------------- | --------------------------------------------------- | ------------------------- |
| `organizations.mock.json`      | Organization     | `orgName`, `status`, `code`                                      | API uses `Name`, `IsActive`, `CreatedAtUtc`, etc.                                  | Mock shape differs from live API                    | P0/P1                     |
| `members.mock.json`            | Members          | legacy `orgId`, `userId`, `displayName`, `roleId`                | API `MemberDto` uses `OrganizationId`, `FullName`, `Role`, omits `UserId`/`RoleId` | Mock gives UI fields live API lacks                 | P0/P1                     |
| `events.mock.json`             | Events           | `location`, `totalSlots`, `budgetLabel`, `riskLevel`, `imageUrl` | API `EventDto` omits several of these                                              | UI confidence from mock is false                    | P1                        |
| `tasks.mock.json`              | Tasks            | flexible UI-style task shape                                     | API supports one assignee and fixed `TaskDto`                                      | Multi-assignee/task detail mismatch                 | P1                        |
| `requests.mock.json`           | Requests         | rich application fields                                          | Entity stores mostly `Content`, `RequestType`, `Status`                            | Request UI may expect fields Backend cannot provide | P1                        |
| `event-categories.mock.json`   | Event categories | extra `leadMemberId`, `isUrgent`, `guidelines`                   | API derives `LeadMemberId`, `LeadName`, task counts                                | Extra fields are not canonical                      | P2                        |
| post/comment mock files        | Posts/comments   | mock-only social fields                                          | Backend/FE live not aligned                                                        | UI works only in mock                               | P2 unless demo uses posts |
| messages mock files            | Messages         | mock-only chat shape                                             | No confirmed Backend API                                                           | Live page breaks                                    | P2 / hide                 |
| finance/resources/report mocks | Admin analytics  | mock-only dashboards                                             | No complete Backend API                                                            | Not E2E                                             | P2 / hide                 |

**Decision:** keep mock data only as fallback/demo filler, but rebuild it to mimic real API wrappers if it is used in the same screens as live API.

For immediate demo recovery, do **not** try to make every mock module real. Instead:

1. Run core demo with live Backend.
2. Hide/avoid mock-only pages.
3. Only update mocks for organization/member/event if you still need offline demo mode.

---

# 7. End-to-end breakpoints

## 1. Mock mode hides Backend failure

* **Chain segment broken:** FE service selection → API call
* **Related files:**

  * `src/Org.Frontend/appsettings.json`
  * `src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs`
  * `src/Org.Frontend/Services/Mocks`
* **Evidence:** `UseMockServices = true`; several live API clients throw.
* **Runtime consequence:** UI renders from mock while real API may be broken.
* **Fastest fix direction:** create an E2E demo config with `UseMockServices = false`; smoke-test only supported live pages.

## 2. Organization field naming mismatch

* **DB/Entity:** `Organization.OrgName`
* **API DTO:** `OrganizationDto.Name`
* **Mock:** `orgName`
* **UI/legacy model:** mixed `Name`, `OrgName`, display name fields
* **Mismatch:** mock shape differs from real API shape.
* **Runtime consequence:** UI code written against mock may fail in live mode.
* **Fastest fix direction:** canonical FE model should map API `Name`; mock must also expose `Name` if used for same service.

## 3. Member DTO missing fields required by FE

* **DB/Entity:** `Member.UserId`, `Member.RoleId`, `Member.DepartmentId`
* **API DTO:** `MemberDto` has `Id`, `OrganizationId`, `DepartmentId`, `StudentCode`, `FullName`, `Email`, `Role`, `IsActive`, `JoinedAtUtc`
* **FE adapter:** `MemberApiClient` sets `UserId = Guid.Empty` and maps role to hardcoded GUIDs.
* **Mismatch:** live API does not provide `UserId`/`RoleId`, but legacy UI/mock expects them.
* **Runtime consequence:** member profile links, friend/message actions, and role assignment can break or silently assign wrong role.
* **Fastest fix direction:** add optional `UserId` and `RoleId` to `src/Org.Shared/Features/Members/MemberContracts.cs`, map them in Backend, then update `MemberApiClient`.

## 4. Role assignment split between enum and DB role IDs

* **DB/Entity:** `Role.Id`, `Role.RoleName`
* **Auth canonical:** `Org.Shared.MemberRole`
* **FE:** hardcoded role GUIDs in `MemberApiClient`
* **Mismatch:** hardcoded GUIDs are not seeded DB role IDs.
* **Runtime consequence:** assigning roles from UI can become wrong or degrade to `Member`.
* **Fastest fix direction:** for demo, use either:

  * enum-based `PUT /api/members/{id}/role`, or
  * real role API `POST /api/organizations/{id}/members/{memberId}/role`.

  Do not use fake role GUIDs.

## 5. Event DTO lacks Entity/UI fields

* **DB/Entity:** `Event.Location`, `Budget`, `TargetParticipants`, `AverageRating`
* **API DTO:** `EventDto` omits these
* **FE/mock:** event cards/details expect location/slots/budget-like fields
* **Mismatch:** Backend has data but API hides it.
* **Runtime consequence:** live event detail/list can show empty/fake values.
* **Fastest fix direction:** add minimal demo fields to `EventDto`: `Location`, `TargetParticipants`, maybe `Budget`, `AverageRating`; update `ContractMapping` and `EventApiClient`.

## 6. Event registration not implemented

* **DB candidates:** `Attendee`, `EventMember`
* **API endpoint:** no confirmed register/unregister endpoint
* **FE:** `EventApiClient.RegisterEventAsync` / `UnregisterEventAsync` throw
* **Mismatch:** UI action exists without Backend.
* **Runtime consequence:** clicking register in live demo fails.
* **Fastest fix direction:** hide/disable register/unregister buttons for deadline unless registration is required.

## 7. Department task CRUD not implemented

* **DB/Entity:** `OrgTask`
* **API:** task CRUD exists under category: `/api/categories/{categoryId}/tasks`
* **FE department service:** expects department task CRUD
* **Mismatch:** Department task management concept does not match Backend route model.
* **Runtime consequence:** department task actions fail live.
* **Fastest fix direction:** demo department task overview only; task CRUD should happen inside event category/task flow.

## 8. Request DTO too rich for Entity

* **DB/Entity:** `Request.Content`, `Request.RequestType`, `Request.Status`
* **API DTO:** `OrganizationRequestDto` has `Title`, `Message`, `DesiredDepartment`, `DesiredPosition`, `Experience`, `Strengths`, `Reason`, review fields, etc.
* **Mismatch:** many fields are not represented as columns.
* **Runtime consequence:** request form/detail may lose data or show null fields.
* **Fastest fix direction:** for demo, store a compact formatted payload in `Content`, and have UI display only guaranteed fields.

## 9. Unsupported live modules are still reachable

* **Modules:** Discover, Messages, Posts, Finance, Reports, Resources depending on navigation
* **FE:** live clients throw or stay mock-only
* **Backend:** no complete API confirmed
* **Runtime consequence:** demo can crash by navigating to unsupported pages.
* **Fastest fix direction:** hide/disable these routes in live demo.

## 10. Seeder/permission instability

* **DB/Entity:** `Role`, `Permission`, `RolePermission`
* **Backend authorization:** role-level fallback plus permission codes
* **Seeder:** generic permissions and likely wrong role indexing
* **Runtime consequence:** role/permission demo may behave inconsistently.
* **Fastest fix direction:** for deadline, seed canonical roles and minimal permissions correctly for one demo organization/user.

---

# 8. P0/P1/P2 list

| ID    | Severity | Problem                                               | Related files/folders                                                    | Evidence                                              | Fastest fix                                          |          Before deadline? | Risk if fixed | Risk if not fixed             |
| ----- | -------: | ----------------------------------------------------- | ------------------------------------------------------------------------ | ----------------------------------------------------- | ---------------------------------------------------- | ------------------------: | ------------- | ----------------------------- |
| P0-01 |       P0 | Real DB connection/startup not verified               | `src/Org.Backend/appsettings.json`, `.env.example`, `Program.cs`         | Connection string placeholder                         | Set local `.env`/user-secrets, run migration + seed  |                       Yes | Low           | Backend cannot demo           |
| P0-02 |       P0 | Mock mode hides E2E failure                           | `src/Org.Frontend/appsettings.json`, `FrontendStartupExtensions.cs`      | `UseMockServices=true`                                | Use live mode for smoke/demo                         |                       Yes | Medium        | False confidence              |
| P0-03 |       P0 | Login/default organization depends on seed membership | `DatabaseSeeder.cs`, auth/org endpoints                                  | `/organizations/default` needs member                 | Use seeded account and verify                        |                       Yes | Low           | User lands on empty/error app |
| P0-04 |       P0 | Member/role mapping can assign wrong role             | `MemberApiClient.cs`, `MemberContracts.cs`, `ContractMapping.cs`         | hardcoded role GUIDs                                  | Use enum or real role ID API; add `RoleId` if needed | Yes, if member admin demo | Medium        | Admin CRUD demo breaks        |
| P0-05 |       P0 | Unsupported live modules reachable                    | `DiscoverApiClient`, `MessageApiClient`, `OverviewApiClient`, nav/routes | live clients throw                                    | Hide/disable for demo                                |                       Yes | Low           | Demo crash                    |
| P0-06 |       P0 | API response shapes not locked                        | `Org.Shared/Features`, API clients                                       | lists/detail/raw mixed                                | Write route/shape matrix and enforce clients         |                       Yes | Low           | FE guesses wrong shape        |
| P1-01 |       P1 | Event DTO omits live Entity fields                    | `EventContracts.cs`, `ContractMapping.cs`, `EventApiClient.cs`           | Entity has fields DTO omits                           | Add minimal fields or simplify UI                    |               Recommended | Medium        | Event UI looks broken         |
| P1-02 |       P1 | Department task CRUD mismatch                         | `DepartmentApiClient.cs`, task endpoints                                 | API supports category tasks, not department task CRUD | Use read-only overview                               |       Yes if page visible | Low           | Actions fail                  |
| P1-03 |       P1 | Request DTO/entity mismatch                           | `Request.cs`, request contracts/endpoints                                | DTO has many non-entity fields                        | Demo only guaranteed fields                          |     If request flow shown | Medium        | Null/lost request fields      |
| P1-04 |       P1 | Seeder role permission bug                            | `DatabaseSeeder.cs`                                                      | role indexing likely wrong                            | Fix indexing and canonical permissions               |               Recommended | Medium        | Permission UI unreliable      |
| P1-05 |       P1 | Mock JSON shape differs from API                      | `Mocks/Data`, `MockDataExporter.cs`                                      | mock uses legacy fields                               | Re-export API-shaped mocks later                     | No, unless mock demo used | Medium        | Mock/live behave differently  |
| P1-06 |       P1 | `TotalMembers` may be stale                           | `DatabaseSeeder.cs`, org mapping                                         | seeded count may not match members                    | Calculate after seeding members                      |                  Optional | Low           | Wrong counts shown            |
| P2-01 |       P2 | Duplicate DTO namespaces                              | `Org.Shared/Contracts`, `Org.Shared/Features`                            | two contract systems                                  | Defer cleanup                                        |                        No | High          | Technical debt remains        |
| P2-02 |       P2 | Messages/finance/reports/resources incomplete         | FE services/pages + Backend missing APIs                                 | mock-only                                             | Exclude                                              |                        No | High          | Out of scope                  |
| P2-03 |       P2 | UI redesign/large refactor                            | Frontend components                                                      | Not needed for E2E                                    | Defer                                                |                        No | High          | None for demo                 |
| P2-04 |       P2 | Full RBAC normalization                               | roles/permissions                                                        | complex                                               | Defer except minimal seed fix                        |                        No | High          | Some permissions imperfect    |

---

# 9. Proposed canonical contract

This is not a perfect architecture. This is the minimum contract to recover demo.

## 9.1 Organization

* **Canonical Entity:** `src/Org.Backend/Domain/Entities/Organization.cs`
* **Canonical DTO:** `src/Org.Shared/Features/Organizations/OrganizationContracts.cs`
* **Canonical endpoints:**

  * `GET /api/organizations`
  * `GET /api/organizations/default`
  * `GET /api/organizations/{id}`
  * `POST /api/organizations`
  * `PUT /api/organizations/{id}`
  * `GET /api/organizations/{id}/public-overview`

### Minimal response DTO fields

```csharp
Id
Name
Description
AvatarUrl
CoverUrl
FoundingDate
Location
TotalMembers
IsActive
CreatedAtUtc
UpdatedAtUtc
```

### Adapter rules

* `Organization.OrgName` → `OrganizationDto.Name`
* `Organization.Status == Active` → `IsActive = true`
* FE must use `Name`, not `orgName`.

### Mock rule

If mock is kept, organization mock must follow API DTO naming or be converted through the same adapter.

---

## 9.2 Member / OrganizationMember

* **Canonical Entity:** `src/Org.Backend/Domain/Entities/Member.cs`
* **Related Entities:** `User`, `Role`, `Department`, `Organization`
* **Canonical endpoints:**

  * `GET /api/organizations/{orgId}/members`
  * `POST /api/organizations/{orgId}/members`
  * `PUT /api/members/{id}/role`
  * `PUT /api/members/{id}/department`
  * `DELETE /api/members/{id}`

### Minimal response DTO fields

Current DTO is insufficient for FE admin/member management. Minimal demo-safe version should be:

```csharp
Id
OrganizationId
UserId
DepartmentId
RoleId
StudentCode
FullName
Email
Role
IsActive
JoinedAtUtc
```

### Adapter rules

* `Member.UserId` must not become `Guid.Empty`.
* `Member.RoleId` must not be replaced by fake hardcoded GUIDs.
* `Role.RoleName` must map to `MemberRole`.
* For fast demo, role assignment should use `MemberRole` enum or real role API, not fake FE role IDs.

### Mock rule

Mock member data must not include fake role IDs unless they correspond to seeded DB role IDs.

---

## 9.3 Event

* **Canonical Entity:** `src/Org.Backend/Domain/Entities/Event.cs`
* **Canonical DTO:** `src/Org.Shared/Features/Events/EventContracts.cs`
* **Canonical endpoints:**

  * `GET /api/organizations/{orgId}/events`
  * `GET /api/events/{id}`
  * `POST /api/events`
  * `PUT /api/events/{id}`
  * `GET /api/events/public`

### Minimal response DTO fields

Current DTO should be expanded for demo if event UI displays location/slots/budget:

```csharp
Id
OrganizationId
Name
Description
StartDate
EndDate
Status
Visibility
Location
TargetParticipants
Budget
AverageRating
Tags
CreatedAtUtc
UpdatedAtUtc
```

### Adapter rules

* `Event.EventName` → `Name`
* `Event.Location` → `Location`
* `Event.TargetParticipants` → `TargetParticipants`
* `Event.Budget` → `Budget`
* `Event.Tags` JSON string → `Tags`

### Mock rule

Mock event cards should not invent `riskLevel`, `budgetLabel`, `totalSlots`, unless those are derived display fields after API mapping.

---

## 9.4 Milestone / Category / Task

* **Canonical Entities:**

  * `Milestone`
  * `EventCategory`
  * `OrgTask`
* **Canonical endpoints:**

  * `GET /api/events/{eventId}/milestones`
  * `GET /api/milestones/{milestoneId}/categories`
  * `GET /api/categories/{categoryId}/tasks`
  * `POST /api/categories/{categoryId}/tasks`
  * `PUT /api/tasks/{taskId}`

### Rules

* Backend supports **one assignee per task**.
* Frontend must not send multi-assignee task requests in live mode.
* Department task CRUD should not be used as canonical; use category task CRUD.

---

## 9.5 User/Auth/Role

* **Canonical auth endpoints:**

  * `POST /api/auth/login`
  * `GET /api/auth/me`
* **Canonical role source:**

  * DB `Role.RoleName`
  * `Org.Shared.MemberRole`

### Required seed role names

```text
President
VicePresident
Manager
Member
```

For deadline, at least:

```text
President
Manager
Member
```

must work for demo users.

### Rule

Do not use arbitrary frontend role GUIDs as canonical role identity.

---

## 9.6 Main Admin CRUD for demo

Use only these CRUD areas:

1. Organizations
2. Members
3. Departments
4. Events
5. Milestones/categories/tasks if needed

Do not include messages, finance, reports, resources, or full posts/comments in the live demo unless separately implemented.

---

# 10. Fix phases

## Phase 0 — Contract audit & DB truth

### Tasks

1. Run Backend build.
2. Configure real local DB connection.
3. Run EF migration.
4. Run seed.
5. Produce endpoint/response matrix for demo routes only.
6. Verify seeded login account.

### Files to inspect/modify

Inspect:

```text
src/Org.Backend/appsettings.json
.env.example
src/Org.Backend/Program.cs
src/Org.Backend/Infrastructure/Database/AppDbContext.cs
src/Org.Backend/Infrastructure/Database/DatabaseSeeder.cs
src/Org.Shared/Features
src/Org.Backend/Features
```

Modify only config if needed.

### Commands

```powershell
dotnet restore StudentOrgManager.slnx
dotnet build StudentOrgManager.slnx

dotnet ef database update --project src\Org.Backend\Org.Backend.csproj

dotnet run --project src\Org.Backend\Org.Backend.csproj -- --seed
dotnet run --project src\Org.Backend\Org.Backend.csproj
```

### What NOT to do

* Do not refactor architecture.
* Do not touch UI.
* Do not rewrite DTOs yet.
* Do not fix P2 modules.

### Done criteria

* Backend starts.
* DB migration succeeds.
* Seed succeeds.
* Login succeeds with seeded user.
* `/api/organizations/default` returns an organization.

### Risks

* Real DB schema may differ from migrations.
* Existing local DB may contain old broken data.

---

## Phase 1 — Fix Backend contract/API

### Tasks

1. Fix only demo-blocking DTO/API mismatches.
2. Add missing `UserId`/`RoleId` to `MemberDto` if member admin flow needs them.
3. Add minimal event fields to `EventDto` if event UI displays them.
4. Fix seeder role/permission inconsistency if role UI is part of demo.
5. Ensure demo account has President/Manager-level access.
6. Keep route shapes stable.

### Files to modify

Likely:

```text
src/Org.Shared/Features/Members/MemberContracts.cs
src/Org.Shared/Features/Events/EventContracts.cs
src/Org.Backend/Features/Common/ContractMapping.cs
src/Org.Backend/Features/Members/*
src/Org.Backend/Features/Events/*
src/Org.Backend/Infrastructure/Database/DatabaseSeeder.cs
```

### Verification

```powershell
dotnet build StudentOrgManager.slnx
dotnet run --project src\Org.Backend\Org.Backend.csproj -- --seed
dotnet run --project src\Org.Backend\Org.Backend.csproj
```

Smoke endpoints:

```text
POST /api/auth/login
GET /api/auth/me
GET /api/organizations/default
GET /api/organizations/{orgId}
GET /api/organizations/{orgId}/members
GET /api/organizations/{orgId}/departments
GET /api/organizations/{orgId}/events
GET /api/events/{eventId}
```

### What NOT to do

* Do not add fake endpoints.
* Do not implement messages/finance/reports.
* Do not redesign role system.
* Do not break existing response wrappers.

### Done criteria

* Core API route matrix works.
* Member DTO returns enough data for FE.
* Event DTO returns enough data for FE.
* Demo user can access organization/member/event data.

### Risks

* Changing shared DTOs requires Frontend build fixes.
* Adding fields is low risk; renaming fields is high risk.

---

## Phase 2 — Fix Frontend service/adapter mapping

### Tasks

1. Set demo config to live mode.
2. Fix API clients to consume actual Backend response shapes.
3. Remove hardcoded role GUID dependency.
4. Update member adapter to use real `UserId`/`RoleId`.
5. Update event adapter to use real event fields.
6. Hide or guard unsupported live modules.

### Files to modify

```text
src/Org.Frontend/appsettings.json
src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs
src/Org.Frontend/Services/ApiClients/MemberApiClient.cs
src/Org.Frontend/Services/ApiClients/EventApiClient.cs
src/Org.Frontend/Services/ApiClients/DepartmentApiClient.cs
src/Org.Frontend/Services/ApiClients/OrganizationServiceApiClient.cs
src/Org.Frontend/Components
src/Org.Frontend/Pages
```

### Verification

```powershell
dotnet build StudentOrgManager.slnx
dotnet run --project src\Org.Frontend\Org.Frontend.csproj
```

Manual smoke:

1. Login.
2. Redirect to default organization.
3. Open organization list/detail.
4. Open member list.
5. Open department list.
6. Open event list/detail.
7. Do one safe create/update if needed.

### What NOT to do

* Do not rewrite UI.
* Do not make mock fallback silently hide live API failure.
* Do not add new dependencies.

### Done criteria

* Frontend runs in live mode.
* Core demo pages no longer depend on mock data.
* No `NotSupportedException` page is reachable during demo.

### Risks

* UI may reveal more null handling bugs once mock is disabled.

---

## Phase 3 — Rebuild mock data according to real API

### Tasks

1. Decide whether mock mode is still needed.
2. If yes, make mock responses follow the same contract as live API.
3. Update `MockDataExporter` to export API-shaped data, not legacy-only shape.
4. Remove mock-only fields from core demo services or derive them after mapping.

### Files to modify

```text
src/Org.Backend/Infrastructure/Database/MockDataExporter.cs
src/Org.Frontend/Services/Mocks/Data/*.mock.json
src/Org.Frontend/Services/Mocks/Models/*.cs
src/Org.Frontend/Services/Mocks/FrontendMockDataStore.cs
```

### What NOT to do

* Do not use mock to define Backend fields.
* Do not make live mode fall back to mock silently.

### Done criteria

* Mock and live services return compatible shapes for core demo modules.

### Risks

* Touching all mocks can consume too much time. Do only core modules.

---

## Phase 4 — End-to-end demo hardening

### Tasks

1. Create a fixed demo script.
2. Seed known accounts.
3. Verify all demo clicks.
4. Add graceful error display for failed API calls.
5. Disable unsupported buttons/routes.

### Files to modify

```text
src/Org.Frontend/Shared/NavMenu*
src/Org.Frontend/Pages/*
src/Org.Frontend/Components/*
src/Org.Backend/Infrastructure/Database/DatabaseSeeder.cs
```

### Done criteria

Demo flow works:

1. Backend starts.
2. Frontend starts.
3. Login works.
4. Organization detail works.
5. Member list works.
6. Department list works.
7. Event list/detail works.
8. At least one CRUD flow works if required.

### Risks

* Last-minute UI changes can introduce layout bugs. Keep changes minimal.

---

## Phase 5 — Minimal UI cleanup

### Tasks

1. Only fix visible blockers.
2. Replace crash buttons with disabled states.
3. Ensure empty states are readable.
4. Do not redesign screens.

### What NOT to do

* No full UI redesign.
* No CSS system refactor.
* No new component library.
* No big layout migration.

### Done criteria

* Demo screens look acceptable.
* No broken action is clickable.
* Empty/null values do not crash the page.

---

# 11. Files for IDE agent

## 1. Files/folders to read first

```text
StudentOrgManager.slnx
src/Org.Backend/Org.Backend.csproj
src/Org.Frontend/Org.Frontend.csproj
src/Org.Shared/Org.Shared.csproj

src/Org.Backend/appsettings.json
src/Org.Backend/Program.cs
src/Org.Backend/Infrastructure/Startup
src/Org.Backend/Infrastructure/Database/AppDbContext.cs
src/Org.Backend/Infrastructure/Database/DatabaseSeeder.cs
src/Org.Backend/Infrastructure/Database/MockDataExporter.cs
src/Org.Backend/Migrations
src/Org.Backend/Domain/Entities

src/Org.Shared/Features
src/Org.Shared/Contracts

src/Org.Backend/Features/Auth
src/Org.Backend/Features/Organizations
src/Org.Backend/Features/OrganizationManagement
src/Org.Backend/Features/Members
src/Org.Backend/Features/Departments
src/Org.Backend/Features/Events
src/Org.Backend/Features/Milestones
src/Org.Backend/Features/EventCategories
src/Org.Backend/Features/Tasks
src/Org.Backend/Features/Requests

src/Org.Frontend/appsettings.json
src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs
src/Org.Frontend/Services/ApiClients
src/Org.Frontend/Services/Mocks
src/Org.Frontend/Pages
src/Org.Frontend/Components
```

## 2. Files/folders allowed to modify in Phase 1

```text
src/Org.Shared/Features/Members/MemberContracts.cs
src/Org.Shared/Features/Events/EventContracts.cs
src/Org.Backend/Features/Common/ContractMapping.cs
src/Org.Backend/Features/Members
src/Org.Backend/Features/Events
src/Org.Backend/Infrastructure/Database/DatabaseSeeder.cs
src/Org.Backend/appsettings.Development.json
```

Only modify the above if the mismatch is confirmed.

## 3. Files/folders forbidden in Phase 1

```text
src/Org.Frontend/Pages
src/Org.Frontend/Components
src/Org.Frontend/wwwroot
src/Org.Frontend/Services/Mocks
src/Org.Shared/Contracts
```

Also forbidden:

* UI redesign
* Full role system rewrite
* New dependencies
* Mock data rewrite
* Messages/finance/reports/resources implementation

## 4. Files/folders to modify in Phase 2

```text
src/Org.Frontend/appsettings.json
src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs
src/Org.Frontend/Services/ApiClients/MemberApiClient.cs
src/Org.Frontend/Services/ApiClients/EventApiClient.cs
src/Org.Frontend/Services/ApiClients/DepartmentApiClient.cs
src/Org.Frontend/Services/ApiClients/OrganizationServiceApiClient.cs
src/Org.Frontend/Services/ApiClients/OrganizationRoleApiClient.cs
src/Org.Frontend/Pages
src/Org.Frontend/Components
```

## 5. Files/folders to re-check after modification

```text
src/Org.Shared/Features
src/Org.Backend/Features/Common/ContractMapping.cs
src/Org.Backend/Features/*/*Endpoints.cs
src/Org.Frontend/Services/ApiClients
src/Org.Frontend/Infrastructure/Startup/FrontendStartupExtensions.cs
src/Org.Frontend/appsettings.json
src/Org.Backend/Infrastructure/Database/DatabaseSeeder.cs
build_log.txt
```

---

# 12. Prompt for IDE agent

```text
You are stabilizing the PBL3 Student Organization Manager repo for an urgent end-to-end demo.

Objective:
Recover a working Backend + Frontend demo as fast as possible. Do not refactor architecture. Do not redesign UI. Do not implement non-demo modules. Focus only on Backend startup, DB migration/seed, auth, organization, member, department, event, milestone/category/task flows.

Context:
The repo has been heavily vibe-coded. Mock data is not trustworthy. Some UI screens render only because mock mode is enabled. The real source of truth must be the database/EF model and Backend contracts, not mock data or currently-rendering UI.

Source-of-truth priority:
1. EF Core Migrations / ModelSnapshot / real DB schema if available
2. EF Entities / AppDbContext
3. Backend DTOs in Org.Shared/Features
4. Backend endpoints in Org.Backend/Features
5. Backend services/use cases
6. Frontend API clients
7. Frontend adapters/view models
8. Mock data
9. UI components/pages

Allowed scope for Phase 0 + Phase 1:
- Inspect the whole repo.
- Modify only Backend/shared-contract files needed to fix demo-blocking API contract mismatches.
- Allowed files:
  - src/Org.Shared/Features/Members/MemberContracts.cs
  - src/Org.Shared/Features/Events/EventContracts.cs
  - src/Org.Backend/Features/Common/ContractMapping.cs
  - src/Org.Backend/Features/Members/*
  - src/Org.Backend/Features/Events/*
  - src/Org.Backend/Infrastructure/Database/DatabaseSeeder.cs
  - src/Org.Backend/appsettings.Development.json if needed for local config

Forbidden changes:
- Do not redesign UI.
- Do not rewrite architecture.
- Do not touch mock data in Phase 1.
- Do not treat mock data as canonical.
- Do not invent endpoints.
- Do not implement messages, finance, reports, resources, or post/comment features.
- Do not add new dependencies.
- Do not rename large folders/namespaces.
- Do not change response wrapper conventions unless absolutely required.

Tasks:
1. Build the solution and record the exact result.
2. Verify Backend config and DB connection.
3. Run EF migration and seed:
   - dotnet ef database update --project src\Org.Backend\Org.Backend.csproj
   - dotnet run --project src\Org.Backend\Org.Backend.csproj -- --seed
4. Create a route/response matrix for these demo endpoints only:
   - POST /api/auth/login
   - GET /api/auth/me
   - GET /api/organizations/default
   - GET /api/organizations/{id}
   - GET /api/organizations/{id}/members
   - GET /api/organizations/{id}/departments
   - GET /api/organizations/{id}/events
   - GET /api/events/{eventId}
   - GET /api/events/{eventId}/milestones
   - GET /api/milestones/{milestoneId}/categories
   - GET /api/categories/{categoryId}/tasks
5. Confirm whether MemberDto needs UserId and RoleId for the Frontend live adapter. If yes, add them as backward-compatible fields and update ContractMapping.
6. Confirm whether EventDto needs Location, TargetParticipants, Budget, AverageRating for the demo UI. If yes, add only these minimal fields and update ContractMapping.
7. Check DatabaseSeeder role creation and role-permission assignment. Fix only if it blocks seeded demo user access.
8. Verify seeded login user, preferably example1@gmail.com / example1, can access default organization and demo endpoints.

Verification commands:
- dotnet restore StudentOrgManager.slnx
- dotnet build StudentOrgManager.slnx
- dotnet ef database update --project src\Org.Backend\Org.Backend.csproj
- dotnet run --project src\Org.Backend\Org.Backend.csproj -- --seed
- dotnet run --project src\Org.Backend\Org.Backend.csproj

Expected output report:
- Build result
- Migration result
- Seed result
- Confirmed demo account
- Endpoint smoke-test result table
- Files changed
- DTO fields added, if any
- Remaining P0 blockers
- Remaining P1 issues
- Do not include generic cleanup suggestions
```
