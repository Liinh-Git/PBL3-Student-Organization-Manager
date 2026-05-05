# API Live Contract Matrix

- Runtime source of truth: `Docs/swagger-live.json` + smoke captures in `Docs/tmp-live-samples-sanitized.json`.
- Environment: backend at `http://localhost:5058`.
- Auth sanity check (2026-05-06): `GET /api/organizations/default` without token returns `401`; `POST /api/auth/login` returns `200`.

## [POST] /api/auth/login

- Status: PASS
- Source:
  - Swagger path: `/api/auth/login`
  - Backend file: `src/Org.Backend/Features/Auth/LoginEndpoint.cs`
  - Shared DTO file: `src/Org.Shared/Features/Auth/*`
- Auth:
  - Anonymous
  - Role/permission: none
- Request:
  - Route params: none
  - Query params: none
  - Body DTO: `LoginRequest`
  - Example request JSON:
```json
{ "email": "example1@gmail.com", "password": "example1" }
```
- Response:
  - Status codes: 200 (401 invalid credentials)
  - DTO: `LoginResponse`
  - Wrapper shape: raw DTO
  - Example response JSON:
```json
{ "accessToken": "***", "expiresAtUtc": "2026-05-05T18:48:04.746559Z", "userId": "f0abd1f8-125e-4e32-931a-8b708b369b5e", "fullName": "User 1", "email": "example1@gmail.com" }
```
  - Important fields: `accessToken`, `expiresAtUtc`, `userId`
- Backend logic:
  - Entities touched: `User`
  - Validation: normalized email + BCrypt verify
  - Side effects: updates `LastLogin`
- Frontend mapping:
  - Interface: `IAuthService`
  - ApiClient: `AuthApiClient`
  - Method: `LoginAsync`
  - ViewModel: auth state provider stores token and claims
  - Page/component: `Components/Pages/Auth/Login.razor`
- Known mismatch: none
- Manual test steps: login form submit; expect 200 and redirect `/home`.
- Phase 2B fix direction: none

## [GET] /api/auth/me

- Status: PASS
- Source:
  - Swagger path: `/api/auth/me`
  - Backend file: `src/Org.Backend/Features/Auth/MeEndpoint.cs`
  - Shared DTO file: `src/Org.Shared/Features/Auth/MeResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: authenticated user
- Request:
  - Route params: none
  - Query params: none
  - Body DTO: none
  - Example request JSON: none
- Response:
  - Status codes: 200, 401
  - DTO: `MeResponse`
  - Wrapper shape: raw DTO
  - Example response JSON:
```json
{ "userId": "f0abd1f8-125e-4e32-931a-8b708b369b5e", "fullName": "User 1", "email": "example1@gmail.com", "status": "Active" }
```
  - Important fields: `userId`, `email`
- Backend logic:
  - Entities touched: `User`
  - Validation: token subject must be GUID
  - Side effects: none
- Frontend mapping:
  - Interface: `IAuthService`
  - ApiClient: `AuthApiClient`
  - Method: `GetMeAsync`
  - ViewModel: claims principal in `FrontendAuthStateProvider`
  - Page/component: auth bootstrap and guarded routes
- Known mismatch: none
- Manual test steps: call with/without bearer; expect 200 with token, 401 without.
- Phase 2B fix direction: none

## [GET] /api/organizations/default

- Status: PASS
- Source:
  - Swagger path: `/api/organizations/default`
  - Backend file: `src/Org.Backend/Features/Organizations/OrganizationEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Organizations/GetDefaultOrganizationResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: authenticated user
- Request:
  - Route params: none
  - Query params: none
  - Body DTO: none
  - Example request JSON: none
- Response:
  - Status codes: 200, 404, 401
  - DTO: `GetDefaultOrganizationResponse`
  - Wrapper shape: `data`
  - Example response JSON:
```json
{ "data": { "id": "d35c9b14-e873-47e7-b4f7-e30c6054925a", "name": "Organization 1", "description": "Description for organization 1" } }
```
  - Important fields: `data.id`
- Backend logic:
  - Entities touched: `Member`, `Organization`
  - Validation: 404 when no org exists
  - Side effects: none
- Frontend mapping:
  - Interface: `IOrganizationContext`
  - ApiClient: `OrganizationApiClient`
  - Method: `GetOrganizationIdAsync`
  - ViewModel: caches org id in-memory
  - Page/component: all org workspace pages
- Known mismatch: none
- Manual test steps: after login open `/org/events`; confirm orgId resolved and API 200.
- Phase 2B fix direction: none

## [GET] /api/organizations

- Status: PASS
- Source:
  - Swagger path: `/api/organizations`
  - Backend file: `src/Org.Backend/Features/Organizations/OrganizationEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Organizations/GetOrganizationsResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: authenticated user
- Request:
  - Route params: none
  - Query params: `search|q`, `isActive`, `page`, `pageSize`
  - Body DTO: none
  - Example request JSON: none
- Response:
  - Status codes: 200, 400, 401
  - DTO: `GetOrganizationsResponse`
  - Wrapper shape: `items` + pagination
  - Example response JSON:
```json
{ "items": [ { "id": "d35c9b14-e873-47e7-b4f7-e30c6054925a", "name": "Organization 1", "description": "Description for organization 1" } ], "totalCount": 6, "page": 1, "pageSize": 20, "search": null, "isActive": null }
```
  - Important fields: `items[]`, `totalCount`
- Backend logic:
  - Entities touched: `Organization`
  - Validation: query parsing and page bounds
  - Side effects: none
- Frontend mapping:
  - Interface: NEEDS_VERIFICATION for direct FE consumer
  - ApiClient: none direct in core pages
  - Method: n/a
  - ViewModel: n/a
  - Page/component: n/a
- Known mismatch: FE currently does not depend on this list for core org context
- Manual test steps: curl with bearer, test filters and invalid page values.
- Phase 2B fix direction: optional for discover/list screens

## [GET] /api/organizations/{id}

- Status: PASS
- Source:
  - Swagger path: `/api/organizations/{id}`
  - Backend file: `src/Org.Backend/Features/Organizations/OrganizationEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Organizations/GetOrganizationByIdResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: member `CanRead`
- Request:
  - Route params: `id`
  - Query params: none
  - Body DTO: none
  - Example request JSON: none
- Response:
  - Status codes: 200, 403, 404
  - DTO: `GetOrganizationByIdResponse`
  - Wrapper shape: `data`
  - Example response JSON:
```json
{ "data": { "id": "d35c9b14-e873-47e7-b4f7-e30c6054925a", "name": "Organization 1", "description": "Description for organization 1", "avatarUrl": "/images/mockimages/org-1.jpg", "coverUrl": "/images/mockimages/org-cover-1.jpg", "foundingDate": "2025-05-05", "location": "Campus 1", "totalMembers": 10, "isActive": true } }
```
  - Important fields: `data.name`, `data.isActive`
- Backend logic:
  - Entities touched: `Organization`, `Member`, `Role`
  - Validation: membership + read permission
  - Side effects: none
- Frontend mapping:
  - Interface: `IOrganizationService`
  - ApiClient: `OrganizationServiceApiClient`
  - Method: used inside `UpdateOrganizationOverviewAsync`
  - ViewModel: intermediate
  - Page/component: `OrganizationOverview.razor`
- Known mismatch: FE overview fields exceed API payload (mission/vision/tags etc.)
- Manual test steps: use member token; call org by id and verify 200.
- Phase 2B fix direction: add null-guards/defaults for missing overview fields

## [GET] /api/organizations/{id}/public-overview

- Status: PASS
- Source:
  - Swagger path: `/api/organizations/{id}/public-overview`
  - Backend file: `src/Org.Backend/Features/Organizations/OrganizationManagementEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Organizations/GetPublicOrganizationOverviewResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: authenticated
- Request:
  - Route params: `id`
  - Query params: none
  - Body DTO: none
  - Example request JSON: none
- Response:
  - Status codes: 200, 404
  - DTO: `GetPublicOrganizationOverviewResponse`
  - Wrapper shape: `data`
  - Example response JSON: see smoke (`data` contains public org fields)
  - Important fields: `name`, `description`, `totalMembers`
- Backend logic:
  - Entities touched: `Organization`
  - Validation: org exists
  - Side effects: none
- Frontend mapping:
  - Interface: `IOrganizationService`
  - ApiClient: `OrganizationServiceApiClient`
  - Method: `GetOrganizationOverviewAsync`
  - ViewModel: `OrganizationOverviewViewModel`
  - Page/component: `OrganizationOverview.razor`
- Known mismatch: FE expects departments/leadership/highlights; API does not return them.
- Manual test steps: open `/org-overview?orgId=<id>`.
- Phase 2B fix direction: enrich overview via extra endpoint calls (members/departments/events)

## [GET] /api/organizations/{id}/permissions/me

- Status: PASS
- Source:
  - Swagger path: `/api/organizations/{id}/permissions/me`
  - Backend file: `src/Org.Backend/Features/Organizations/OrganizationManagementEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Organizations/GetOrganizationPermissionsMeResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: authenticated; guest result possible when not member
- Request:
  - Route params: `id`
  - Query params: none
  - Body DTO: none
- Response:
  - Status codes: 200, 404
  - DTO: `GetOrganizationPermissionsMeResponse`
  - Wrapper shape: `data`
  - Example response JSON:
```json
{ "data": { "isAuthenticated": true, "isMember": true, "canAccessWorkspace": true, "canEditOverview": true, "canManageMembers": true, "canCreateEvents": true, "canViewRequests": true, "canReviewRequests": true, "canManageRoles": true, "canManageDepartments": true, "memberRole": "President", "permissionCodes": ["module.permission.1"] } }
```
  - Important fields: permission booleans
- Backend logic:
  - Entities touched: `Organization`, `Member`, `RolePermission`, `Permission`
  - Validation: org exists
  - Side effects: none
- Frontend mapping:
  - Interface: `IOrganizationService`, `IOrganizationRoleService`, `IRequestService`
  - ApiClient: organization/request/role clients
  - Method: permission gates
  - ViewModel: `OrganizationViewerPermissionViewModel`
  - Page/component: overview, member list, org requests
- Known mismatch: none
- Manual test steps: compare permission booleans with visible UI actions.
- Phase 2B fix direction: centralize permission-gating in pages

## [GET] /api/organizations/{id}/roles

- Status: PASS
- Source:
  - Swagger path: `/api/organizations/{id}/roles`
  - Backend file: `src/Org.Backend/Features/Organizations/OrganizationManagementEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Organizations/GetOrganizationRolesResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: `CanManageRoles`
- Request:
  - Route params: `id`
  - Query params: none
  - Body DTO: none
- Response:
  - Status codes: 200, 403, 404
  - DTO: `GetOrganizationRolesResponse`
  - Wrapper shape: `items`
  - Example response JSON: smoke has role entries with `id/name/isProtected/permissionCodes`
  - Important fields: `id`, `name`, `isProtected`
- Backend logic:
  - Entities touched: `Role`, `RolePermission`, `Permission`, `Member`
  - Validation: org exists and capability
  - Side effects: none
- Frontend mapping:
  - Interface: `IOrganizationRoleService`
  - ApiClient: `OrganizationRoleApiClient`
  - Method: `GetRolesAsync`
  - ViewModel: `OrganizationRoleViewModel`
  - Page/component: `MemberList.razor` role manager
- Known mismatch: none
- Manual test steps: open members page as president/manager and inspect role table.
- Phase 2B fix direction: none

## [GET] /api/organizations/{orgId}/members

- Status: PASS
- Source:
  - Swagger path: `/api/organizations/{orgId}/members`
  - Backend file: `src/Org.Backend/Features/Members/MemberEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Members/GetMembersResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: `CanRead`
- Request:
  - Route params: `orgId`
  - Query params: none
  - Body DTO: none
- Response:
  - Status codes: 200, 403, 404
  - DTO: `GetMembersResponse`
  - Wrapper shape: `items`
  - Example response JSON: smoke items include `studentCode`, numeric `role`, `departmentId`
  - Important fields: `id`, `role`, `departmentId`, `email`
- Backend logic:
  - Entities touched: `Member`, `User`, `Role`, `Organization`
  - Validation: org exists + permission
  - Side effects: none
- Frontend mapping:
  - Interface: `IMemberService`
  - ApiClient: `MemberApiClient`
  - Method: `GetMembers`
  - ViewModel: maps to legacy `Org.Shared.Contracts.MemberDto`
  - Page/component: `MemberList.razor`
- Known mismatch: legacy adapter sets `UserId=Guid.Empty`; can affect self-protection logic.
- Manual test steps: open `/org/members`; verify list populates.
- Phase 2B fix direction: remove legacy contract dependency and map real user id

## [POST] /api/organizations/{id}/members/{memberId}/role

- Status: PARTIAL
- Source:
  - Swagger path: `/api/organizations/{id}/members/{memberId}/role`
  - Backend file: `src/Org.Backend/Features/Organizations/OrganizationManagementEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Organizations/AssignOrganizationRoleRequest.cs`
- Auth:
  - Bearer required
  - Role/permission: `CanManageRoles`
- Request:
  - Route params: `id`, `memberId`
  - Query params: none
  - Body DTO: `AssignOrganizationRoleRequest { roleId }`
  - Example request JSON:
```json
{ "roleId": "3b6f2a81-eda8-4ef9-b5f3-a6eb9a17523c" }
```
- Response:
  - Status codes: 200, 400, 403, 404
  - DTO: `MemberDto` raw
  - Wrapper shape: raw DTO
- Backend logic:
  - Entities touched: `Member`, `Role`, `RolePermission`
  - Validation: role belongs organization
  - Side effects: updates role assignment
- Frontend mapping:
  - Interface: `IOrganizationRoleService`
  - ApiClient: `OrganizationRoleApiClient`
  - Method: `AssignRoleToMemberAsync`
  - ViewModel: no mapping required on success
  - Page/component: `MemberList.razor` via `AssignRoleDialog`
- Known mismatch: none; this is safer than fake GUID enum bridge.
- Manual test steps: assign role in members page and refresh role badge.
- Phase 2B fix direction: keep this as canonical role assignment path

## [GET] /api/organizations/{orgId}/departments

- Status: PASS
- Source:
  - Swagger path: `/api/organizations/{orgId}/departments`
  - Backend file: `src/Org.Backend/Features/Departments/DepartmentEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Departments/GetDepartmentsResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: `DepartmentAuthorization.CanRead`
- Request:
  - Route params: `orgId`
  - Query params: `search|q`, `isActive`, `page`, `pageSize`
  - Body DTO: none
- Response:
  - Status codes: 200, 400, 403, 404
  - DTO: `GetDepartmentsResponse`
  - Wrapper shape: `items` + pagination
  - Example response JSON: smoke includes `id`, `code`, `name`, `managerMemberId`, `memberCount`
  - Important fields: `items[].id/name/memberCount`
- Backend logic:
  - Entities touched: `Department`, `Member`, `Organization`
  - Validation: query parsing; org exists
  - Side effects: none
- Frontend mapping:
  - Interface: `IDepartmentService`
  - ApiClient: `DepartmentApiClient`
  - Method: `GetDepartments`
  - ViewModel: feature dto -> legacy `DepartmentDto`
  - Page/component: `DepartmentList.razor`, `MemberList.razor`
- Known mismatch: none for list; task CRUD methods are unsupported elsewhere.
- Manual test steps: open `/org/departments`; verify cards/list.
- Phase 2B fix direction: keep list live; guard task CRUD buttons

## [GET] /api/organizations/{orgId}/events

- Status: PASS
- Source:
  - Swagger path: `/api/organizations/{orgId}/events`
  - Backend file: `src/Org.Backend/Features/Events/EventEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Events/GetOrganizationEventsResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: `CanRead`
- Request:
  - Route params: `orgId`
  - Query params: none
  - Body DTO: none
- Response:
  - Status codes: 200, 403, 404
  - DTO: `GetOrganizationEventsResponse`
  - Wrapper shape: `items`
  - Example response JSON: smoke includes `id/name/status/startDate/endDate/taskCount/completedTaskCount`
  - Important fields: `status`, task counters
- Backend logic:
  - Entities touched: `Event`, `Milestone`, `EventCategory`, `OrgTask`, `Organization`, `Member`
  - Validation: org + permission
  - Side effects: none
- Frontend mapping:
  - Interface: `IEventService`
  - ApiClient: `EventApiClient`
  - Method: `GetEventsAsync`
  - ViewModel: `EventTreeNodeDto -> EventViewModel`
  - Page/component: `EventList.razor`
- Known mismatch: FE status label mapper collapses `Draft/Planning` to `UPCOMING`.
- Manual test steps: open `/org/events`; filter by status.
- Phase 2B fix direction: fix `EventApiClient.ToStatusLabel` for `Planning`

## [GET] /api/events/{id}

- Status: PASS
- Source:
  - Swagger path: `/api/events/{id}`
  - Backend file: `src/Org.Backend/Features/Events/EventEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Events/GetEventByIdResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: `CanRead`
- Request:
  - Route params: `id`
  - Query params: none
  - Body DTO: none
- Response:
  - Status codes: 200, 403, 404
  - DTO: `GetEventByIdResponse`
  - Wrapper shape: `data`
  - Example response JSON: smoke returns `data` event DTO
- Backend logic:
  - Entities touched: `Event`, `Member`
  - Validation: event exists + permission
  - Side effects: none
- Frontend mapping:
  - Interface: `IEventService`
  - ApiClient: `EventApiClient`
  - Method: `GetEventDetailAsync`
  - ViewModel: event detail composed with milestone/category calls
  - Page/component: `EventDetail.razor`, `TaskBoard.razor` permission derivation
- Known mismatch: none
- Manual test steps: open `/org/events/{eventId}`.
- Phase 2B fix direction: none

## [GET] /api/events/{eventId}/milestones

- Status: PASS
- Source:
  - Swagger path: `/api/events/{eventId}/milestones`
  - Backend file: `src/Org.Backend/Features/Milestones/MilestoneEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Milestones/GetMilestonesResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: `CanRead`
- Request:
  - Route params: `eventId`
  - Query params: none
  - Body DTO: none
- Response:
  - Status codes: 200, 403, 404
  - DTO: `GetMilestonesResponse`
  - Wrapper shape: `items`
  - Example response JSON: smoke includes `id,eventId,name,startDate,endDate,sortOrder,status`
- Backend logic:
  - Entities touched: `Event`, `Milestone`, `Member`
  - Validation: event exists + permission
  - Side effects: none
- Frontend mapping:
  - Interface: `IMilestoneService`
  - ApiClient: `MilestoneApiClient`
  - Method: `GetMilestonesAsync`
  - ViewModel: `MilestoneDto -> MilestoneViewModel`
  - Page/component: `EventDetail.razor`
- Known mismatch: none
- Manual test steps: event detail timeline tab.
- Phase 2B fix direction: none

## [GET] /api/milestones/{milestoneId}/categories

- Status: PASS
- Source:
  - Swagger path: `/api/milestones/{milestoneId}/categories`
  - Backend file: `src/Org.Backend/Features/EventCategories/EventCategoryEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/EventCategories/GetEventCategoriesResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: `CanRead`
- Request:
  - Route params: `milestoneId`
  - Query params: none
  - Body DTO: none
- Response:
  - Status codes: 200, 403, 404
  - DTO: `GetEventCategoriesResponse`
  - Wrapper shape: `items`
  - Example response JSON: smoke includes task counts and lead fields
- Backend logic:
  - Entities touched: `Milestone`, `Event`, `EventCategory`, `OrgTask`, `Department`, `Member`
  - Validation: milestone exists + permission
  - Side effects: none
- Frontend mapping:
  - Interface: `IEventCategoryService`
  - ApiClient: `EventCategoryApiClient`
  - Method: `GetCategoriesAsync`
  - ViewModel: `EventCategoryDto -> EventCategoryViewModel`
  - Page/component: `EventDetail.razor`
- Known mismatch: none
- Manual test steps: event detail loads categories per milestone.
- Phase 2B fix direction: none

## [GET] /api/categories/{categoryId}/tasks

- Status: PASS
- Source:
  - Swagger path: `/api/categories/{categoryId}/tasks`
  - Backend file: `src/Org.Backend/Features/Tasks/TaskEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Tasks/GetTasksResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: `CanRead`
- Request:
  - Route params: `categoryId`
  - Query params: none
  - Body DTO: none
- Response:
  - Status codes: 200, 403, 404
  - DTO: `GetTasksResponse`
  - Wrapper shape: `items`
  - Example response JSON: smoke includes `id,title,status,priority,assigneeMemberId,dueDate`
- Backend logic:
  - Entities touched: `EventCategory`, `Milestone`, `Event`, `OrgTask`
  - Validation: category exists + permission
  - Side effects: none
- Frontend mapping:
  - Interface: `ITaskService`
  - ApiClient: `TaskApiClient`
  - Method: `GetTasksAsync`
  - ViewModel: `TaskDto -> TaskViewModel`
  - Page/component: `TaskBoard.razor`
- Known mismatch: task status is enum in API; UI has string comparisons (`TODO/IN_PROGRESS/DONE`).
- Manual test steps: open task board route.
- Phase 2B fix direction: normalize status handling in task UI + adapters

## [PUT] /api/tasks/{taskId}/status

- Status: PARTIAL
- Source:
  - Swagger path: `/api/tasks/{taskId}/status`
  - Backend file: `src/Org.Backend/Features/Tasks/TaskEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Tasks/UpdateTaskStatusRequest.cs`
- Auth:
  - Bearer required
  - Role/permission: `CanRead` and (planner or assignee)
- Request:
  - Route params: `taskId`
  - Query params: none
  - Body DTO: `UpdateTaskStatusRequest`
  - Example request JSON:
```json
{ "status": "Done" }
```
- Response:
  - Status codes: 200, 400, 403, 404
  - DTO: `TaskDto` raw
  - Wrapper shape: raw DTO
- Backend logic:
  - Entities touched: `OrgTask`, `Member`
  - Validation: transition only `Todo -> InProgress -> Done`
  - Side effects: notification to assignee
- Frontend mapping:
  - Interface: `ITaskService`
  - ApiClient: `TaskApiClient`
  - Method: `UpdateTaskStatusAsync`
  - ViewModel: no body mapping on return
  - Page/component: `TaskBoard.razor`
- Known mismatch: FE sends string states and can attempt unsupported transitions via drag/drop.
- Manual test steps: move TODO -> IN_PROGRESS -> DONE; ensure DONE cannot go backward.
- Phase 2B fix direction: enforce legal transitions client-side

## [GET] /api/notifications

- Status: PASS
- Source:
  - Swagger path: `/api/notifications`
  - Backend file: `src/Org.Backend/Features/Notifications/NotificationEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Notifications/GetNotificationsResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: current receiver only
- Request:
  - Route params: none
  - Query params: `page`, `pageSize`, `isRead`, `type`
  - Body DTO: none
- Response:
  - Status codes: 200, 400, 401
  - DTO: `GetNotificationsResponse`
  - Wrapper shape: `items` + `totalCount` + `unreadCount` + pagination
  - Example response JSON:
```json
{ "items": [], "totalCount": 0, "unreadCount": 0, "page": 1, "pageSize": 20 }
```
- Backend logic:
  - Entities touched: `Notification`, `User(Actor)`
  - Validation: pagination bounds
  - Side effects: none
- Frontend mapping:
  - Interface: `INotificationService`
  - ApiClient: `NotificationService`
  - Method: `GetNotificationsAsync`
  - ViewModel: direct DTO use
  - Page/component: `NotificationBadge.razor`
- Known mismatch: none
- Manual test steps: open bell popover; verify list and unread count call.
- Phase 2B fix direction: none

## [GET] /api/organizations/{id}/requests

- Status: PASS
- Source:
  - Swagger path: `/api/organizations/{id}/requests`
  - Backend file: `src/Org.Backend/Features/Requests/OrganizationRequestEndpoints.cs`
  - Shared DTO file: `src/Org.Shared/Features/Requests/GetOrganizationRequestsResponse.cs`
- Auth:
  - Bearer required
  - Role/permission: `CanViewRequests`
- Request:
  - Route params: `id`
  - Query params: `status` (default `PENDING`)
  - Body DTO: none
- Response:
  - Status codes: 200, 403, 404
  - DTO: `GetOrganizationRequestsResponse`
  - Wrapper shape: `items`
  - Example response JSON: smoke contains rich request item fields (requester, title/message, review metadata)
- Backend logic:
  - Entities touched: `Request`, `User`, `Member`, `RolePermission`, `Permission`
  - Validation: org exists; status filter parse
  - Side effects: none
- Frontend mapping:
  - Interface: `IRequestService`
  - ApiClient: `RequestApiClient`
  - Method: `GetPendingRequestsAsync`
  - ViewModel: `OrganizationRequestDto -> RequestViewModel`
  - Page/component: `OrganizationRequests.razor`
- Known mismatch: none
- Manual test steps: open `/org/requests` as authorized role.
- Phase 2B fix direction: none

## [POST] /api/organizations/{id}/requests

- Status: PARTIAL
- Source:
  - Swagger path: `/api/organizations/{id}/requests`
  - Backend file: `src/Org.Backend/Features/Requests/OrganizationRequestEndpoints.cs`
  - Shared DTO file: `CreateOrganizationRequestSubmissionRequest`
- Auth:
  - Bearer required
  - Role/permission: must be current org member
- Request:
  - Route params: `id`
  - Query params: none
  - Body DTO: `CreateOrganizationRequestSubmissionRequest`
  - Example request JSON:
```json
{ "requestType": "JOIN", "title": "Join organization request", "message": "I want to join", "desiredDepartment": "Media", "desiredPosition": "Member", "experience": "...", "strengths": "...", "reason": "..." }
```
- Response:
  - Status codes: 201, 400, 401, 403, 404
  - DTO: `OrganizationRequestDto` raw
  - Wrapper shape: raw DTO
- Backend logic:
  - Entities touched: `Request`, `User`, `Member`
  - Validation: title/message required
  - Side effects: reviewer notifications
- Frontend mapping:
  - Interface: `IRequestService`
  - ApiClient: `RequestApiClient`
  - Method: `SubmitJoinRequestAsync` / `SubmitOrganizationRequestAsync`
  - ViewModel: request form VMs
  - Page/component: join/request dialogs, member list request dialog
- Known mismatch: none
- Manual test steps: submit dialog and verify appears in pending list.
- Phase 2B fix direction: add user-visible success/error handling per status code

## [POST] /api/organizations/requests/{requestId}/review

- Status: PARTIAL
- Source:
  - Swagger path: `/api/organizations/requests/{requestId}/review`
  - Backend file: `src/Org.Backend/Features/Requests/OrganizationRequestEndpoints.cs`
  - Shared DTO file: `ReviewOrganizationRequestSubmissionRequest`
- Auth:
  - Bearer required
  - Role/permission: `CanReviewRequests`
- Request:
  - Route params: `requestId`
  - Query params: none
  - Body DTO: `ReviewOrganizationRequestSubmissionRequest`
  - Example request JSON:
```json
{ "decision": "APPROVE", "responseMessage": "Approved" }
```
- Response:
  - Status codes: 200, 401, 403, 404, 409
  - DTO: `OrganizationRequestDto` raw
  - Wrapper shape: raw DTO
- Backend logic:
  - Entities touched: `Request`, `User`, `Member`, `RolePermission`, `Permission`
  - Validation: pending-only, decision must be approve/reject
  - Side effects: requester notification
- Frontend mapping:
  - Interface: `IRequestService`
  - ApiClient: `RequestApiClient`
  - Method: `ApproveRequestAsync` / `RejectRequestAsync`
  - ViewModel: `RequestDetailViewModel`
  - Page/component: `OrganizationRequests.razor`
- Known mismatch: none
- Manual test steps: approve/reject pending request and reload list.
- Phase 2B fix direction: keep current flow; add optimistic UI guards

## [MISSING] Event registration endpoints

- Status: UNSUPPORTED
- Source:
  - Swagger path: no `POST/DELETE` registration endpoints found for `IEventService.RegisterEventAsync/UnregisterEventAsync`
  - Backend file: none in `src/Org.Backend/Features/Events`
  - Shared DTO file: none for registration action in FE service path
- Auth:
  - n/a
- Request: n/a
- Response: n/a
- Backend logic: n/a
- Frontend mapping:
  - Interface: `IEventService`
  - ApiClient: `EventApiClient`
  - Method: `RegisterEventAsync`, `UnregisterEventAsync` throw `NotSupportedException`
  - Page/component: `PublicEventDetail.razor`
- Known mismatch: FE exposes action but backend contract missing.
- Manual test steps: click register/unregister in live mode and observe unsupported error.
- Phase 2B fix direction: disable/hide buttons for demo
