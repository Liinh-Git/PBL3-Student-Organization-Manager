# API Backend Logic Map

- Canonical sources: `Docs/swagger-live.json` + endpoint implementations under `src/Org.Backend/Features/**`.
- Scope: demo-relevant live endpoints plus FE-called write endpoints used in core org workspace flows.

## Auth

### POST /api/auth/login
- Backend endpoint file: `src/Org.Backend/Features/Auth/LoginEndpoint.cs`
- Request DTO: `src/Org.Shared/Features/Auth/LoginRequest.cs` (schema `OrgSharedFeaturesAuthLoginRequest`)
- Response DTO: `src/Org.Shared/Features/Auth/LoginResponse.cs`
- Entities touched: `User`
- Query/include logic: lookup by normalized email.
- Validation: invalid credentials => 401.
- Permission check: `AllowAnonymous()`.
- Side effects: updates `User.LastLogin`; issues JWT via `IJwtTokenService`.
- Response wrapper: raw DTO (no `data`/`items`).

### GET /api/auth/me
- Backend endpoint file: `src/Org.Backend/Features/Auth/MeEndpoint.cs`
- Request DTO: none
- Response DTO: `src/Org.Shared/Features/Auth/MeResponse.cs`
- Entities touched: `User`
- Query/include logic: read `NameIdentifier` claim, fetch user by PK.
- Validation: invalid subject => 401; missing user => 404.
- Permission check: `AuthSchemes(JwtBearerDefaults.AuthenticationScheme)`.
- Side effects: none.
- Response wrapper: raw DTO.

## Users/Current User

### GET /api/users/me
- Backend endpoint file: `src/Org.Backend/Features/Users/UserEndpoints.cs` (`GetCurrentUserProfileEndpoint`)
- Request DTO: none
- Response DTO: `GetCurrentUserProfileResponse` (`data` wrapper)
- Entities touched: `User`
- Validation: invalid token subject => 401; user not found => 404.
- Permission check: bearer required.
- Side effects: none.

### GET /api/users/me/organizations
- Backend endpoint file: `src/Org.Backend/Features/Users/UserEndpoints.cs` (`GetMyOrganizationsEndpoint`)
- Response DTO: `GetMyOrganizationsResponse` (`items` list)
- Entities touched: `Member`, `Organization`, `Role`
- Logic: membership query ordered by `JoinDate` desc.
- Permission check: bearer required.
- Side effects: none.

## Organizations

### GET /api/organizations/default
- Backend endpoint file: `src/Org.Backend/Features/Organizations/OrganizationEndpoints.cs`
- Response DTO: `GetDefaultOrganizationResponse` (`data`)
- Entities touched: `Member`, `Organization`
- Logic: first joined org for caller, fallback first org alphabetically.
- Validation: no organizations => 404.
- Permission check: bearer required.
- Side effects: none.

### GET /api/organizations
- Backend endpoint file: `.../OrganizationEndpoints.cs` (`GetOrganizationsEndpoint`)
- Request source: query `search|q`, `isActive`, `page`, `pageSize`.
- Response DTO: `GetOrganizationsResponse` (`items` + pagination fields).
- Entities touched: `Organization`
- Validation: invalid bool/int query params => 400; `pageSize>100` => 400.
- Permission check: bearer required.
- Side effects: none.

### GET /api/organizations/{id}
- Backend endpoint file: `.../OrganizationEndpoints.cs` (`GetOrganizationByIdEndpoint`)
- Response DTO: `GetOrganizationByIdResponse` (`data`)
- Entities touched: `Organization`, `Member`, `Role`
- Validation: org missing => 404.
- Permission check: membership `CanRead` via `OrganizationAuthorization`.
- Side effects: none.

### GET /api/organizations/{id}/public-overview
- Backend endpoint file: `src/Org.Backend/Features/Organizations/OrganizationManagementEndpoints.cs`
- Response DTO: `GetPublicOrganizationOverviewResponse` (`data`)
- Entities touched: `Organization`
- Validation: org missing => 404.
- Permission check: bearer required (route is named public but not anonymous).
- Side effects: none.

### PUT /api/organizations/{id}
- Backend endpoint file: `.../OrganizationEndpoints.cs` (`UpdateOrganizationEndpoint`)
- Request DTO: `UpdateOrganizationRequest`
- Response DTO: raw `OrganizationDto`
- Entities touched: `Organization`, `Member`, `Role`
- Validation: name length, duplicate name, org existence.
- Permission check: `CanPlan` (Manager+).
- Side effects: updates org profile/state fields.

## Organization Roles/Permissions

### GET /api/organizations/{id}/permissions/me
- Backend endpoint file: `OrganizationManagementEndpoints.cs` (`GetOrganizationPermissionsMeEndpoint`)
- Response DTO: `GetOrganizationPermissionsMeResponse` (`data`)
- Entities touched: `Organization`, `Member`, `Role`, `RolePermission`, `Permission`
- Logic: guest payload for authenticated non-members; member payload from permission catalog.
- Permission check: bearer required.
- Side effects: none.

### GET /api/organizations/{id}/permissions
- Backend endpoint file: `OrganizationManagementEndpoints.cs` (`GetOrganizationPermissionsCatalogEndpoint`)
- Response DTO: `GetOrganizationPermissionsCatalogResponse` (`items`)
- Entities touched: `Organization`, `Member`, `RolePermission`, `Permission`
- Permission check: caller must be member and `CanManageRoles`.
- Side effects: may call `EnsureCatalogExistsAsync` to seed permission catalog.

### GET /api/organizations/{id}/roles
- Backend endpoint file: `OrganizationManagementEndpoints.cs` (`GetOrganizationRolesEndpoint`)
- Response DTO: `GetOrganizationRolesResponse` (`items`)
- Entities touched: `Role`, `RolePermission`, `Permission`, `Member`
- Logic: includes assigned-member counts per role.
- Permission check: `CanManageRoles`.
- Side effects: none.

### POST /api/organizations/{id}/members/{memberId}/role
- Backend endpoint file: `OrganizationManagementEndpoints.cs` (`AssignRoleToOrganizationMemberEndpoint`)
- Request DTO: `AssignOrganizationRoleRequest`
- Response DTO: `Org.Shared.Features.Members.MemberDto` (raw)
- Entities touched: `Member`, `Role`, `RolePermission`, `Permission`
- Validation: target member in org; target role belongs org.
- Permission check: `CanManageRoles`; protected role assignment requires `VicePresident+`.
- Side effects: updates member `RoleId`.

### PUT /api/members/{id}/role
- Backend endpoint file: `src/Org.Backend/Features/Members/MemberEndpoints.cs` (`UpdateMemberRoleEndpoint`)
- Request DTO: `UpdateMemberRoleRequest` (enum role)
- Response DTO: `MemberDto` raw
- Entities touched: `Member`, `Role`, `User`
- Validation: member exists.
- Permission check: `CanDelete` (VicePresident+).
- Side effects: may auto-create role if role name missing; notification `NotifyMemberRoleChanged`.

## Members

### GET /api/organizations/{orgId}/members
- Backend endpoint file: `MemberEndpoints.cs` (`GetMembersEndpoint`)
- Response DTO: `GetMembersResponse` (`items`)
- Entities touched: `Member`, `User`, `Role`
- Permission check: `CanRead`.
- Side effects: none.

### POST /api/organizations/{orgId}/members
- Backend endpoint file: `MemberEndpoints.cs` (`CreateMemberEndpoint`)
- Request DTO: `CreateMemberRequest`
- Response DTO: `MemberDto` raw (`201`)
- Entities touched: `User`, `Member`, `Department`
- Validation: name/email; optional department must belong org; duplicates.
- Permission check: `CanPlan`.
- Side effects: creates/re-activates user and membership.

### DELETE /api/members/{id}
- Backend endpoint file: `MemberEndpoints.cs` (`DeleteMemberEndpoint`)
- Response: `204`
- Entities touched: `Member`, `Department`
- Permission check: `CanDelete`.
- Side effects: soft-delete membership; clears department manager assignments; notification.

### POST /api/organizations/{orgId}/leave
- Backend endpoint file: `MemberEndpoints.cs` (`LeaveOrganizationEndpoint`)
- Response: `204`
- Entities touched: `Member`, `Department`, `Organization`
- Validation: membership exists; last top leader cannot leave (409).
- Side effects: soft-delete caller membership; clears manager links; updates `Organization.TotalMembers`.

## Departments

### GET /api/organizations/{orgId}/departments
- Backend endpoint file: `Departments/DepartmentEndpoints.cs` (`GetDepartmentsEndpoint`)
- Query: `search|q`, `isActive`, `page`, `pageSize`
- Response DTO: `GetDepartmentsResponse` (`items` + pagination)
- Entities touched: `Department`, `Member`, `Organization`
- Permission check: `DepartmentAuthorization.CanRead`.
- Side effects: none.

### POST /api/departments
- Backend endpoint file: `DepartmentEndpoints.cs` (`CreateDepartmentEndpoint`)
- Request DTO: `CreateDepartmentRequest`
- Response DTO: `DepartmentDto` raw (`201`)
- Entities touched: `Department`, `Organization`, `Member`
- Validation: normalized name/code, unique code per org, manager membership.
- Permission check: `CanWrite` (Manager+).
- Side effects: creates department row.

### PUT /api/departments/{id}
- Backend endpoint file: `DepartmentEndpoints.cs` (`UpdateDepartmentEndpoint`)
- Request DTO: `UpdateDepartmentRequest`
- Response DTO: `DepartmentDto` raw
- Entities touched: `Department`, `Member`
- Validation: unique code, manager membership.
- Permission check: `CanWrite`.
- Side effects: can soft-delete via `IsActive=false` in body mapping.

### GET /api/departments/{id}/members
- Backend endpoint file: `DepartmentEndpoints.cs` (`GetDepartmentMembersEndpoint`)
- Response DTO: `GetDepartmentMembersResponse` (`items`)
- Entities touched: `Department`, `Member`, `User`, `Role`
- Permission check: `CanRead`.
- Side effects: none.

### PUT /api/departments/{id}/manager
- Backend endpoint file: `DepartmentEndpoints.cs` (`UpdateDepartmentManagerEndpoint`)
- Request DTO: `UpdateDepartmentManagerRequest`
- Response DTO: `DepartmentDto` raw
- Entities touched: `Department`, `Member`
- Permission check: `CanAssign` (Manager+).
- Side effects: updates `ManagerId`.

### GET /api/departments/{id}/tasks/overview
- Backend endpoint file: `DepartmentEndpoints.cs` (`GetDepartmentTasksOverviewEndpoint`)
- Response DTO: `GetDepartmentTasksOverviewResponse`
- Entities touched: `Department`, `OrgTask`, `Member`, `User`
- Permission check: `CanRead`.
- Side effects: none.

## Events

### GET /api/organizations/{orgId}/events
- Backend endpoint file: `Events/EventEndpoints.cs` (`GetOrganizationEventsEndpoint`)
- Response DTO: `GetOrganizationEventsResponse` (`items`)
- Entities touched: `Event`, `Milestone`, `EventCategory`, `OrgTask`, `Member`
- Permission check: `CanRead`.
- Side effects: none.

### GET /api/events/{id}
- Backend endpoint file: `EventEndpoints.cs` (`GetEventByIdEndpoint`)
- Response DTO: `GetEventByIdResponse` (`data`)
- Entities touched: `Event`, `Member`
- Permission check: `CanRead`.
- Side effects: none.

### POST /api/events
- Backend endpoint file: `EventEndpoints.cs` (`CreateEventEndpoint`)
- Request DTO: `CreateEventRequest`
- Response DTO: raw `EventDto` (`201`)
- Entities touched: `Event`, `Organization`, `Member`
- Validation: name/date range.
- Permission check: `CanPlan`.
- Side effects: inserts event with `Status=Draft`.

### PUT /api/events/{id}
- Backend endpoint file: `EventEndpoints.cs` (`UpdateEventEndpoint`)
- Request DTO: `UpdateEventRequest`
- Response DTO: raw `EventDto`
- Entities touched: `Event`, `Attendee`
- Permission check: `CanPlan`.
- Side effects: updates event, notifies non-cancelled attendees.

### DELETE /api/events/{id}
- Backend endpoint file: `EventEndpoints.cs` (`DeleteEventEndpoint`)
- Response: `204`
- Entities touched: `Event`, `Milestone`, `EventCategory`, `OrgTask`, `Attendee`
- Permission check: `CanDelete`.
- Side effects: cascade soft-delete child tree; sends cancellation notifications.

### GET /api/events/{id}/public
- Backend endpoint file: `EventEndpoints.cs` (`GetPublicEventByIdEndpoint`)
- Response DTO: `GetEventByIdResponse` (`data`)
- Entities touched: `Event`, `Member`
- Permission check: bearer required globally; non-public events additionally require `CanRead` membership.
- Side effects: none.

## Milestones

### GET /api/events/{eventId}/milestones
- Backend endpoint file: `Milestones/MilestoneEndpoints.cs` (`GetMilestonesEndpoint`)
- Response DTO: `GetMilestonesResponse` (`items`)
- Entities touched: `Event`, `Milestone`, `Member`
- Permission check: `CanRead`.
- Side effects: none.

### POST /api/events/{eventId}/milestones
- Backend endpoint file: `MilestoneEndpoints.cs` (`CreateMilestoneEndpoint`)
- Request DTO: `CreateMilestoneRequest`
- Response DTO: raw `MilestoneDto` (`201`)
- Validation: route/body eventId match; date bounds within event.
- Permission check: `CanPlan`.
- Side effects: inserts milestone.

## Event Categories

### GET /api/milestones/{milestoneId}/categories
- Backend endpoint file: `EventCategories/EventCategoryEndpoints.cs` (`GetEventCategoriesEndpoint`)
- Response DTO: `GetEventCategoriesResponse` (`items`)
- Entities touched: `Milestone`, `Event`, `EventCategory`, `Department`, `Member`, `User`, `OrgTask`
- Permission check: `CanRead`.
- Side effects: none.

### GET /api/categories/{id}
- Backend endpoint file: `EventCategoryEndpoints.cs` (`GetEventCategoryByIdEndpoint`)
- Response DTO: `GetEventCategoryByIdResponse` (`data`)
- Entities touched: `EventCategory`, `Milestone`, `Event`, `OrgTask`
- Permission check: `CanRead`.
- Side effects: none.

### POST /api/milestones/{milestoneId}/categories
- Backend endpoint file: `EventCategoryEndpoints.cs` (`CreateEventCategoryEndpoint`)
- Request DTO: `CreateEventCategoryRequest`
- Response DTO: raw `EventCategoryDto` (`201`)
- Permission check: `CanPlan`.
- Side effects: inserts category.

## Tasks

### GET /api/categories/{categoryId}/tasks
- Backend endpoint file: `Tasks/TaskEndpoints.cs` (`GetTasksEndpoint`)
- Response DTO: `GetTasksResponse` (`items`)
- Entities touched: `EventCategory`, `Milestone`, `Event`, `OrgTask`
- Permission check: `CanRead`.
- Side effects: none.

### POST /api/categories/{categoryId}/tasks
- Backend endpoint file: `TaskEndpoints.cs` (`CreateTaskEndpoint`)
- Request DTO: `CreateTaskRequest`
- Response DTO: raw `TaskDto` (`201`)
- Validation: route/body categoryId match; due date within milestone range; assignee belongs org.
- Permission check: `CanPlan`.
- Side effects: inserts task.

### PUT /api/tasks/{taskId}/status
- Backend endpoint file: `TaskEndpoints.cs` (`UpdateTaskStatusEndpoint`)
- Request DTO: `UpdateTaskStatusRequest`
- Response DTO: raw `TaskDto`
- Entities touched: `OrgTask`, `EventCategory`, `Milestone`, `Event`, `Member`
- Validation: transition only `Todo -> InProgress -> Done`.
- Permission check: member with `CanRead`, and either planner or assigned member.
- Side effects: status change + notification to assignee.

## Requests

### GET /api/organizations/{id}/requests
- Backend endpoint file: `Requests/OrganizationRequestEndpoints.cs` (`GetOrganizationRequestsEndpoint`)
- Query: `status` (`PENDING` default, `APPROVED`, `REJECTED`, `ALL`)
- Response DTO: `GetOrganizationRequestsResponse` (`items`)
- Entities touched: `Request`, `User`, `Member`, `RolePermission`, `Permission`
- Permission check: `CanViewRequests` capability.
- Side effects: none.

### POST /api/organizations/{id}/requests
- Backend endpoint file: `OrganizationRequestEndpoints.cs` (`CreateOrganizationRequestEndpoint`)
- Request DTO: `CreateOrganizationRequestSubmissionRequest`
- Response DTO: raw `OrganizationRequestDto` (`201`)
- Validation: member-only; title/message presence.
- Permission check: must be organization member.
- Side effects: inserts request, notifies reviewers.

### POST /api/organizations/requests/{requestId}/review
- Backend endpoint file: `OrganizationRequestEndpoints.cs` (`ReviewOrganizationRequestEndpoint`)
- Request DTO: `ReviewOrganizationRequestSubmissionRequest`
- Response DTO: raw `OrganizationRequestDto`
- Validation: pending-only; decision in `APPROVE|REJECT`.
- Permission check: `CanReviewRequests` capability.
- Side effects: updates status/content metadata, notifies requester.

## Notifications

### GET /api/notifications
- Backend endpoint file: `Notifications/NotificationEndpoints.cs` (`GetNotificationsEndpoint`)
- Request DTO: `GetNotificationsRequest` (query-bound)
- Response DTO: `GetNotificationsResponse` (`items` + `totalCount` + `unreadCount` + pagination)
- Entities touched: `Notification`, `User(Actor)`
- Validation: page/pageSize bounds.
- Permission check: bearer + receiver match by token subject.
- Side effects: none.

### PUT /api/notifications/{id}/read
- Backend endpoint file: `NotificationEndpoints.cs` (`MarkAsReadEndpoint`)
- Response DTO: `MarkAsReadResponse` (`data`)
- Entities touched: `Notification`, `User`
- Side effects: sets `IsRead`, `ReadAt`.

### PUT /api/notifications/read-all
- Backend endpoint file: `NotificationEndpoints.cs` (`MarkAllAsReadEndpoint`)
- Response DTO: `MarkAllAsReadResponse`
- Entities touched: `Notification`
- Side effects: bulk read-flag update.

### DELETE /api/notifications/{id}
- Backend endpoint file: `NotificationEndpoints.cs` (`DeleteNotificationEndpoint`)
- Response: `204`
- Entities touched: `Notification`
- Side effects: soft delete.

## Notes

- `OrganizationAuthorization` role thresholds (shared across modules): `CanRead >= Member`, `CanPlan >= Manager`, `CanDelete >= VicePresident`.
- Soft-delete global filter in `AppDbContext` applies to all `BaseEntity` descendants unless `IgnoreQueryFilters()` is used.
