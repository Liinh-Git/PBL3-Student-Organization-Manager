# SHARED_CONTRACT_CONSISTENCY_MATRIX

## Purpose
This is the shared-contract consistency checkpoint for frontend skeleton task 3C-4 and final cross-layer docs 3C-5. It ensures all modules have proper contract alignment across backend, shared contracts, and future frontend layers.

## CORE Modules

| Module | Status | Backend Routes | Shared Contract File | Request DTOs | Response DTOs | Required Permissions | Future Frontend Service | Future Adapter | Future Pages/Components | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| Auth | CORE | POST /api/auth/login, POST /api/auth/register, GET /api/auth/me | AuthContracts.cs.TODO | LoginRequest, RegisterRequest | AuthUserDto, AuthTokenResponse, CurrentUserResponse | Public (login/register), JWT (me) | authService.js | userAdapter.js | LoginPage.jsx, RegisterPage.jsx | JWT implementation deferred |
| Users | CORE | GET /api/users/me, PUT /api/users/me, PUT /api/users/me/change-password, GET /api/users/me/organizations, GET /api/users/me/events, GET /api/users/me/discover/organizations | UserContracts.cs.TODO | UpdateUserProfileRequest, ChangePasswordRequest | UserProfileDto, MyOrganizationDto, MyEventDto, DiscoverOrganizationDto | JWT | userService.js | userAdapter.js | UserProfilePage.jsx, UserSettingsPage.jsx, UserOrganizationsPage.jsx, UserEventsPage.jsx | getMyOrganizations belongs to userService |
| Organizations | CORE | GET /api/organizations, POST /api/organizations, GET /api/organizations/default, GET /api/organizations/{id}, PUT /api/organizations/{id}, GET /api/organizations/{id}/public-overview | OrganizationContracts.cs.TODO | CreateOrganizationRequest, UpdateOrganizationRequest | OrganizationDto, OrganizationSummaryDto, OrganizationPublicOverviewDto | JWT (list/create), org.workspace.access (get), org.overview.write (update), Public (public-overview) | organizationService.js | organizationAdapter.js | OrgOverviewPage.jsx, OrgCard.jsx, OrgSwitcher.jsx | OrgName uniqueness is service-level |
| Members | CORE | GET /api/organizations/{orgId}/members, POST /api/organizations/{orgId}/members, PUT /api/members/{id}/department, DELETE /api/members/{id} | MemberContracts.cs.TODO | AddMemberRequest, UpdateMemberDepartmentRequest | MemberDto | org.workspace.access (list), org.members.manage (add/update/delete) | memberService.js | memberAdapter.js | OrgMembersPage.jsx | Role assignment belongs to RolesPermissions module |
| Departments | CORE | GET /api/organizations/{orgId}/departments, POST /api/organizations/{orgId}/departments, GET /api/departments/{id}, PUT /api/departments/{id}, DELETE /api/departments/{id} | DepartmentContracts.cs.TODO | CreateDepartmentRequest, UpdateDepartmentRequest | DepartmentDto | org.workspace.access (list/get), org.departments.manage (create/update/delete) | departmentService.js | departmentAdapter.js | OrgDepartmentsPage.jsx | ManagerId points to Member |
| Events | CORE | GET /api/organizations/{orgId}/events, POST /api/organizations/{orgId}/events, GET /api/events/{id}, PUT /api/events/{id}, DELETE /api/events/{id}, GET /api/events/public, GET /api/events/{id}/public | EventContracts.cs.TODO | CreateEventRequest, UpdateEventRequest | EventDto, EventSummaryDto, EventPublicDto | org.workspace.access (list/get), org.events.create (create), org.events.manage (update/delete), Public (public) | eventService.js | eventAdapter.js | OrgEventsPage.jsx, OrgEventDetailPage.jsx, EventCard.jsx | Do not fake TargetParticipants/Budget/AverageRating |
| Milestones | CORE | GET /api/events/{eventId}/milestones, POST /api/events/{eventId}/milestones, GET /api/milestones/{id}, PUT /api/milestones/{id}, DELETE /api/milestones/{id} | MilestoneContracts.cs.TODO | CreateMilestoneRequest, UpdateMilestoneRequest | MilestoneDto | org.workspace.access (list/get), org.events.manage (create/update/delete) | milestoneService.js | milestoneAdapter.js | MilestonePanel.jsx (inside EventDetail) | OrderIndex maintained for timeline |
| EventCategories | CORE | GET /api/milestones/{milestoneId}/categories, POST /api/milestones/{milestoneId}/categories, GET /api/categories/{id}, PUT /api/categories/{id}, DELETE /api/categories/{id} | CategoryContracts.cs.TODO | CreateEventCategoryRequest, UpdateEventCategoryRequest | EventCategoryDto | org.workspace.access (list/get), org.events.manage (create/update/delete) | categoryService.js | categoryAdapter.js | CategoryPanel.jsx (inside EventDetail) | CategoryDto may include tasks[] array |
| Tasks | CORE | POST /api/categories/{categoryId}/tasks, GET /api/tasks/{taskId}, PUT /api/tasks/{taskId}, DELETE /api/tasks/{taskId}, PUT /api/tasks/{taskId}/status, PUT /api/tasks/{taskId}/assign | TaskContracts.cs.TODO | CreateTaskRequest, UpdateTaskRequest, UpdateTaskStatusRequest, AssignTaskRequest | TaskDto | org.workspace.access (get), org.events.manage (create/update/delete/status/assign) | taskService.js | taskAdapter.js | TaskCard.jsx (inside EventDetail) | Task is CORE inside EventDetail, only /org/tasks board is PROTOTYPE_ONLY |
| Requests | CORE | GET /api/organizations/{orgId}/requests, POST /api/organizations/{orgId}/requests, GET /api/requests/{requestId}, POST /api/organizations/requests/{requestId}/review | RequestContracts.cs.TODO | CreateRequestRequest, ReviewRequestRequest | RequestDto | org.requests.view (list/get), JWT (create), org.requests.review/approve (review) | requestService.js | requestAdapter.js | OrgRequestsPage.jsx | Supports join organization workflow |
| Notifications | CORE | GET /api/notifications, GET /api/notifications/unread-count, POST /api/notifications/{id}/read, POST /api/notifications/read-all | NotificationContracts.cs.TODO | None | NotificationDto, UnreadCountResponse, MarkNotificationReadResponse | JWT | notificationService.js | notificationAdapter.js | NotificationBadge.jsx | REST only, SignalR optional future |
| RolesPermissions | CORE | GET /api/organizations/{orgId}/permissions/me, GET /api/organizations/{orgId}/permissions, GET /api/organizations/{orgId}/roles, POST /api/organizations/{orgId}/roles, PUT /api/organizations/roles/{roleId}, DELETE /api/organizations/roles/{roleId}, POST /api/organizations/{orgId}/members/{memberId}/role | RoleContracts.cs.TODO | CreateRoleRequest, UpdateRoleRequest, AssignRoleToMemberRequest | PermissionDto, MyPermissionsResponse, RoleDto | JWT (permissions/me), org.roles.view (list), org.roles.create/update/delete/assign (CRUD/assign) | roleService.js | roleAdapter.js (if needed) | OrgRolesPage.jsx | RoleId is canonical, permissions/me must normalize to string[] |

## SUPPORTING Modules

| Module | Status | Backend Routes | Shared Contract File | Request DTOs | Response DTOs | Required Permissions | Future Frontend Service | Future Adapter | Future Pages/Components | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| Friends | SUPPORTING | GET /api/friends, GET /api/friends/requests, POST /api/friends/requests, POST /api/friends/requests/{id}/accept, POST /api/friends/requests/{id}/reject | FriendContracts.cs.TODO | SendFriendRequestRequest | FriendDto, FriendRequestDto | JWT | friendService.js | friendAdapter.js (if needed) | UserFriendsPage.jsx | SenderId != ReceiverId enforced at service level |
| Discover | SUPPORTING | GET /api/discover/organizations, GET /api/discover/events | DiscoverContracts.cs.TODO | None | DiscoverOrganizationDto, DiscoverEventDto | JWT | discoverService.js | discoverAdapter.js (if needed) | UserDiscoverPage.jsx | No mock fallback |

## DB_FOUNDATION_ONLY Modules

| Module | Status | Backend Routes | Shared Contract File | Request DTOs | Response DTOs | Required Permissions | Future Frontend Service | Future Adapter | Future Pages/Components | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| EventMembers | DB_FOUNDATION_ONLY | None in base prototype | README.md only | None | None | N/A | None | None | None | Event staff/organizer, no working UI/API in base prototype |
| Attendees | DB_FOUNDATION_ONLY | None in base prototype | README.md only | None | None | N/A | None | None | None | Event participant/registration, no working UI/API in base prototype |
| DigitalAssets | DB_FOUNDATION_ONLY | None in base prototype | README.md only | None | None | N/A | None | None | None | Event file/asset, no upload API in base prototype |
| EventRatings | DB_FOUNDATION_ONLY | None in base prototype | README.md only | None | None | N/A | None | None | None | Event rating, no working UI/API in base prototype |
| EventReports | DB_FOUNDATION_ONLY | None in base prototype | README.md only | None | None | N/A | None | None | None | Event report, Reports page remains PROTOTYPE_ONLY |
| Resources | DB_FOUNDATION_ONLY | None in base prototype | README.md only | None | None | N/A | None | None | None | Organization resource, Resources page remains PROTOTYPE_ONLY |
| ActivityHistory | DB_FOUNDATION_ONLY | None in base prototype | README.md only | None | None | N/A | None | None | None | Activity feed/log, no working UI/API in base prototype |

## Permission Correction Notes

### Canonical Permission Keys
The following permission keys are canonical and approved:
- `org.overview.read`
- `org.overview.write`
- `org.workspace.access`
- `org.members.manage`
- `org.roles.view`
- `org.roles.create`
- `org.roles.update`
- `org.roles.delete`
- `org.roles.assign`
- `org.events.create`
- `org.events.manage`
- `org.departments.manage`
- `org.requests.view`
- `org.requests.review`
- `org.requests.approve`

### Non-Canonical Permission Keys (PROPOSED_ONLY)
The following permission keys appeared in BACKEND_FEATURE_CONSISTENCY_MATRIX.md but are NOT canonical:
- `org.members.view` → Use `org.workspace.access` instead
- `org.events.view` → Use `org.workspace.access` instead
- `org.departments.view` → Use `org.workspace.access` instead

### Permission Mapping Corrections
- **Members list/read**: Use `org.workspace.access` (not `org.members.view`)
- **Members manage**: Use `org.members.manage`
- **Departments list/read**: Use `org.workspace.access` (not `org.departments.view`)
- **Departments manage**: Use `org.departments.manage`
- **Events list/read inside org workspace**: Use `org.workspace.access` (not `org.events.view`)
- **Event create**: Use `org.events.create`
- **Event update/delete/milestone/category/task CRUD**: Use `org.events.manage`
- **Requests list**: Use `org.requests.view`
- **Requests review**: Use `org.requests.review` / `org.requests.approve`
- **Roles list**: Use `org.roles.view`
- **Role create/update/delete/assign**: Use `org.roles.create` / `org.roles.update` / `org.roles.delete` / `org.roles.assign`
- **Notifications**: JWT required (no org-specific permission)
- **Public events/org overview**: Public or authenticated-public depending on endpoint

## Consistency Verification

### Matches PHASE_3C_REQUIREMENTS_SPEC.md
✅ All CORE modules (12) have full contract skeleton
✅ All SUPPORTING modules (2) have full contract skeleton
✅ All DB_FOUNDATION_ONLY modules (7) have README only
✅ All EXCLUDED modules are not created
✅ All PROTOTYPE_ONLY pages are documented

### Matches BACKEND_FEATURE_CONSISTENCY_MATRIX.md
✅ All backend routes are documented in shared contracts
✅ All request/response DTOs are documented
✅ All permissions are corrected to canonical keys
✅ Non-canonical view keys marked as PROPOSED_ONLY

### Matches DOMAIN_ENTITY_LOCK_V1.md
✅ All domain entities are correctly mapped to contracts
✅ All enums are correctly referenced
✅ All relationships are preserved

### No Invented Routes
✅ All routes match approved route list from PHASE_3C_REQUIREMENTS_SPEC.md
✅ No /org/tasks aggregate endpoint invented
✅ No list-by-category task endpoint invented

### No Excluded Modules Created
✅ Posts module not created
✅ Comments module not created
✅ Messages/Chat working module not created
✅ Finance working module not created

### Task Module Clarity
✅ Task module is CORE inside EventDetail tree
✅ Only /org/tasks aggregate board is PROTOTYPE_ONLY
✅ Clear distinction documented

### EventMember and Attendee Treatment
✅ EventMember is DB_FOUNDATION_ONLY
✅ Attendee is DB_FOUNDATION_ONLY
✅ No working UI/API in base prototype
✅ Database foundation preserved

### Resources/EventRatings/EventReports/ActivityHistory Treatment
✅ All are DB_FOUNDATION_ONLY or PROTOTYPE_ONLY
✅ Not working modules
✅ Database foundation preserved

## End of SHARED_CONTRACT_CONSISTENCY_MATRIX.md
