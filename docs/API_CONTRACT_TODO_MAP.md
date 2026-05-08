# API_CONTRACT_TODO_MAP

## Purpose
Complete mapping of backend routes → shared contracts → frontend services → adapters → pages/components → permissions for all CORE and SUPPORTING modules.

## Legend
- ✅ = Skeleton created with TODO notes
- ⚠️ = Placeholder only
- ❌ = Intentionally not implemented
- 📝 = Notes/documentation only

---

## Auth Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/auth/login | POST | Auth/Endpoints/LoginEndpoint.cs.TODO | LoginRequest | AuthTokenResponse | authService.login | userAdapter.toUserProfileViewModel | LoginPage.jsx | Public | ✅ | JWT implementation deferred |
| /api/auth/register | POST | Auth/Endpoints/RegisterEndpoint.cs.TODO | RegisterRequest | AuthUserDto | authService.register | None | RegisterPage.jsx | Public | ✅ | JWT implementation deferred |
| /api/auth/me | GET | Auth/Endpoints/GetCurrentUserEndpoint.cs.TODO | None | CurrentUserResponse | authService.getCurrentUser | userAdapter.toUserProfileViewModel | Multiple | JWT | ✅ | JWT implementation deferred |

---

## Users Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/users/me | GET | Users/Endpoints/GetMeEndpoint.cs.TODO | None | UserProfileDto | userService.getMe | userAdapter.toUserProfileViewModel | UserProfilePage.jsx | JWT | ✅ | User profile |
| /api/users/me | PUT | Users/Endpoints/UpdateMeEndpoint.cs.TODO | UpdateUserProfileRequest | UserProfileDto | userService.updateMe | userAdapter.toUserProfileViewModel | UserSettingsPage.jsx | JWT | ✅ | Update profile |
| /api/users/me/change-password | PUT | Users/Endpoints/ChangePasswordEndpoint.cs.TODO | ChangePasswordRequest | None | userService.changePassword | None | UserSettingsPage.jsx | JWT | ✅ | Change password |
| /api/users/me/organizations | GET | Users/Endpoints/GetMyOrganizationsEndpoint.cs.TODO | None | MyOrganizationDto[] | userService.getMyOrganizations | userAdapter.toMyOrganizationViewModel | UserOrganizationsPage.jsx | JWT | ✅ | getMyOrganizations belongs to userService |
| /api/users/me/events | GET | Users/Endpoints/GetMyEventsEndpoint.cs.TODO | None | MyEventDto[] | userService.getMyEvents | userAdapter.toMyEventViewModel | UserEventsPage.jsx | JWT | ✅ | User's events |
| /api/users/me/discover/organizations | GET | Users/Endpoints/DiscoverMyOrganizationsEndpoint.cs.TODO | None | DiscoverOrganizationDto[] | userService.discoverMyOrganizations | userAdapter.toDiscoverOrganizationViewModel | UserDiscoverPage.jsx | JWT | ✅ | Discover orgs |

---

## Organizations Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/organizations | GET | Organizations/Endpoints/ListOrganizationsEndpoint.cs.TODO | None | OrganizationSummaryDto[] | organizationService.listOrganizations | organizationAdapter.toOrganizationSummaryViewModel | Multiple | JWT | ✅ | List orgs |
| /api/organizations | POST | Organizations/Endpoints/CreateOrganizationEndpoint.cs.TODO | CreateOrganizationRequest | OrganizationDto | organizationService.createOrganization | organizationAdapter.toOrganizationViewModel | Multiple | JWT | ✅ | Create org |
| /api/organizations/default | GET | Organizations/Endpoints/GetDefaultOrganizationEndpoint.cs.TODO | None | OrganizationDto | organizationService.getDefaultOrganization | organizationAdapter.toOrganizationViewModel | Multiple | JWT | ✅ | Default org |
| /api/organizations/{id} | GET | Organizations/Endpoints/GetOrganizationByIdEndpoint.cs.TODO | None | OrganizationDto | organizationService.getOrganizationById | organizationAdapter.toOrganizationViewModel | OrgOverviewPage.jsx | org.workspace.access | ✅ | Get org detail |
| /api/organizations/{id} | PUT | Organizations/Endpoints/UpdateOrganizationEndpoint.cs.TODO | UpdateOrganizationRequest | OrganizationDto | organizationService.updateOrganization | organizationAdapter.toOrganizationViewModel | OrgOverviewPage.jsx | org.overview.write | ✅ | Update org |
| /api/organizations/{id}/public-overview | GET | Organizations/Endpoints/GetPublicOverviewEndpoint.cs.TODO | None | OrganizationPublicOverviewDto | organizationService.getPublicOverview | organizationAdapter.toOrganizationPublicOverviewViewModel | Multiple | Public | ✅ | Public overview |

---

## Members Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/organizations/{orgId}/members | GET | Members/Endpoints/GetOrganizationMembersEndpoint.cs.TODO | None | MemberDto[] | memberService.getOrganizationMembers | memberAdapter.toMemberViewModel | OrgMembersPage.jsx | org.workspace.access | ✅ | List members |
| /api/organizations/{orgId}/members | POST | Members/Endpoints/AddMemberEndpoint.cs.TODO | AddMemberRequest | MemberDto | memberService.addMember | memberAdapter.toMemberViewModel | OrgMembersPage.jsx | org.members.manage | ✅ | Add member |
| /api/members/{id}/department | PUT | Members/Endpoints/UpdateMemberDepartmentEndpoint.cs.TODO | UpdateMemberDepartmentRequest | MemberDto | memberService.updateMemberDepartment | memberAdapter.toMemberViewModel | OrgMembersPage.jsx | org.members.manage | ✅ | Update dept |
| /api/members/{id} | DELETE | Members/Endpoints/RemoveMemberEndpoint.cs.TODO | None | None | memberService.removeMember | None | OrgMembersPage.jsx | org.members.manage | ✅ | Remove member |

---

## RolesPermissions Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/organizations/{orgId}/permissions/me | GET | RolesPermissions/Endpoints/GetMyPermissionsEndpoint.cs.TODO | None | MyPermissionsResponse | roleService.getMyPermissions | roleAdapter.toPermissionViewModel | Multiple | JWT | ✅ | Must normalize to string[] |
| /api/organizations/{orgId}/permissions | GET | RolesPermissions/Endpoints/GetOrganizationPermissionsEndpoint.cs.TODO | None | PermissionDto[] | roleService.getOrganizationPermissions | roleAdapter.toPermissionViewModel | OrgRolesPage.jsx | org.roles.view | ✅ | List permissions |
| /api/organizations/{orgId}/roles | GET | RolesPermissions/Endpoints/GetOrganizationRolesEndpoint.cs.TODO | None | RoleDto[] | roleService.getOrganizationRoles | roleAdapter.toRoleViewModel | OrgRolesPage.jsx | org.roles.view | ✅ | List roles |
| /api/organizations/{orgId}/roles | POST | RolesPermissions/Endpoints/CreateRoleEndpoint.cs.TODO | CreateRoleRequest | RoleDto | roleService.createRole | roleAdapter.toRoleViewModel | OrgRolesPage.jsx | org.roles.create | ✅ | Create role |
| /api/organizations/roles/{roleId} | PUT | RolesPermissions/Endpoints/UpdateRoleEndpoint.cs.TODO | UpdateRoleRequest | RoleDto | roleService.updateRole | roleAdapter.toRoleViewModel | OrgRolesPage.jsx | org.roles.update | ✅ | Update role |
| /api/organizations/roles/{roleId} | DELETE | RolesPermissions/Endpoints/DeleteRoleEndpoint.cs.TODO | None | None | roleService.deleteRole | None | OrgRolesPage.jsx | org.roles.delete | ✅ | Delete role |
| /api/organizations/{orgId}/members/{memberId}/role | POST | RolesPermissions/Endpoints/AssignRoleToMemberEndpoint.cs.TODO | AssignRoleToMemberRequest | MemberDto | roleService.assignRoleToMember | memberAdapter.toMemberViewModel | OrgMembersPage.jsx | org.roles.assign | ✅ | Assign role (belongs to roleService) |

---

## Departments Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/organizations/{orgId}/departments | GET | Departments/Endpoints/GetOrganizationDepartmentsEndpoint.cs.TODO | None | DepartmentDto[] | departmentService.getOrganizationDepartments | departmentAdapter.toDepartmentViewModel | OrgDepartmentsPage.jsx | org.workspace.access | ✅ | List departments |
| /api/organizations/{orgId}/departments | POST | Departments/Endpoints/CreateDepartmentEndpoint.cs.TODO | CreateDepartmentRequest | DepartmentDto | departmentService.createDepartment | departmentAdapter.toDepartmentViewModel | OrgDepartmentsPage.jsx | org.departments.manage | ✅ | Create dept |
| /api/departments/{id} | GET | Departments/Endpoints/GetDepartmentByIdEndpoint.cs.TODO | None | DepartmentDto | departmentService.getDepartmentById | departmentAdapter.toDepartmentViewModel | OrgDepartmentsPage.jsx | org.workspace.access | ✅ | Get dept detail |
| /api/departments/{id} | PUT | Departments/Endpoints/UpdateDepartmentEndpoint.cs.TODO | UpdateDepartmentRequest | DepartmentDto | departmentService.updateDepartment | departmentAdapter.toDepartmentViewModel | OrgDepartmentsPage.jsx | org.departments.manage | ✅ | Update dept |
| /api/departments/{id} | DELETE | Departments/Endpoints/DeleteDepartmentEndpoint.cs.TODO | None | None | departmentService.deleteDepartment | None | OrgDepartmentsPage.jsx | org.departments.manage | ✅ | Delete dept |

---

## Events Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/organizations/{orgId}/events | GET | Events/Endpoints/GetOrganizationEventsEndpoint.cs.TODO | None | EventSummaryDto[] | eventService.getOrganizationEvents | eventAdapter.toEventSummaryViewModel | OrgEventsPage.jsx | org.workspace.access | ✅ | List org events |
| /api/organizations/{orgId}/events | POST | Events/Endpoints/CreateEventEndpoint.cs.TODO | CreateEventRequest | EventDto | eventService.createEvent | eventAdapter.toEventViewModel | OrgEventsPage.jsx | org.events.create | ✅ | Create event |
| /api/events/{id} | GET | Events/Endpoints/GetEventByIdEndpoint.cs.TODO | None | EventDto | eventService.getEventById | eventAdapter.toEventViewModel | OrgEventDetailPage.jsx | org.workspace.access | ✅ | Get event detail |
| /api/events/{id} | PUT | Events/Endpoints/UpdateEventEndpoint.cs.TODO | UpdateEventRequest | EventDto | eventService.updateEvent | eventAdapter.toEventViewModel | OrgEventDetailPage.jsx | org.events.manage | ✅ | Update event |
| /api/events/{id} | DELETE | Events/Endpoints/DeleteEventEndpoint.cs.TODO | None | None | eventService.deleteEvent | None | OrgEventsPage.jsx | org.events.manage | ✅ | Delete event |
| /api/events/public | GET | Events/Endpoints/GetPublicEventsEndpoint.cs.TODO | None | EventPublicDto[] | eventService.getPublicEvents | eventAdapter.toEventPublicViewModel | PublicEventsPage.jsx | Public | ✅ | Public events |
| /api/events/{id}/public | GET | Events/Endpoints/GetPublicEventByIdEndpoint.cs.TODO | None | EventPublicDto | eventService.getPublicEventById | eventAdapter.toEventPublicViewModel | PublicEventDetailPage.jsx | Public | ✅ | Public event detail |

---

## Milestones Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/events/{eventId}/milestones | GET | Milestones/Endpoints/GetEventMilestonesEndpoint.cs.TODO | None | MilestoneDto[] | milestoneService.getEventMilestones | milestoneAdapter.toMilestoneViewModel | OrgEventDetailPage.jsx | org.workspace.access | ✅ | List milestones |
| /api/events/{eventId}/milestones | POST | Milestones/Endpoints/CreateMilestoneEndpoint.cs.TODO | CreateMilestoneRequest | MilestoneDto | milestoneService.createMilestone | milestoneAdapter.toMilestoneViewModel | MilestoneFormModal.jsx | org.events.manage | ✅ | Create milestone |
| /api/milestones/{id} | GET | Milestones/Endpoints/GetMilestoneByIdEndpoint.cs.TODO | None | MilestoneDto | milestoneService.getMilestoneById | milestoneAdapter.toMilestoneViewModel | OrgEventDetailPage.jsx | org.workspace.access | ✅ | Get milestone |
| /api/milestones/{id} | PUT | Milestones/Endpoints/UpdateMilestoneEndpoint.cs.TODO | UpdateMilestoneRequest | MilestoneDto | milestoneService.updateMilestone | milestoneAdapter.toMilestoneViewModel | MilestoneFormModal.jsx | org.events.manage | ✅ | Update milestone |
| /api/milestones/{id} | DELETE | Milestones/Endpoints/DeleteMilestoneEndpoint.cs.TODO | None | None | milestoneService.deleteMilestone | None | OrgEventDetailPage.jsx | org.events.manage | ✅ | Delete milestone |

---

## EventCategories Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/milestones/{milestoneId}/categories | GET | EventCategories/Endpoints/GetMilestoneCategoriesEndpoint.cs.TODO | None | EventCategoryDto[] | categoryService.getMilestoneCategories | categoryAdapter.toCategoryViewModel | OrgEventDetailPage.jsx | org.workspace.access | ✅ | CategoryDto may include tasks[] |
| /api/milestones/{milestoneId}/categories | POST | EventCategories/Endpoints/CreateCategoryEndpoint.cs.TODO | CreateEventCategoryRequest | EventCategoryDto | categoryService.createCategory | categoryAdapter.toCategoryViewModel | CategoryFormModal.jsx | org.events.manage | ✅ | Create category |
| /api/categories/{id} | GET | EventCategories/Endpoints/GetCategoryByIdEndpoint.cs.TODO | None | EventCategoryDto | categoryService.getCategoryById | categoryAdapter.toCategoryViewModel | OrgEventDetailPage.jsx | org.workspace.access | ✅ | Get category |
| /api/categories/{id} | PUT | EventCategories/Endpoints/UpdateCategoryEndpoint.cs.TODO | UpdateEventCategoryRequest | EventCategoryDto | categoryService.updateCategory | categoryAdapter.toCategoryViewModel | CategoryFormModal.jsx | org.events.manage | ✅ | Update category |
| /api/categories/{id} | DELETE | EventCategories/Endpoints/DeleteCategoryEndpoint.cs.TODO | None | None | categoryService.deleteCategory | None | OrgEventDetailPage.jsx | org.events.manage | ✅ | Delete category |

---

## Tasks Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/categories/{categoryId}/tasks | POST | Tasks/Endpoints/CreateTaskEndpoint.cs.TODO | CreateTaskRequest | TaskDto | taskService.createTask | taskAdapter.toTaskViewModel | TaskFormModal.jsx | org.events.manage | ✅ | Create task, response includes TaskDto for local append |
| /api/tasks/{taskId} | GET | Tasks/Endpoints/GetTaskByIdEndpoint.cs.TODO | None | TaskDto | taskService.getTaskById | taskAdapter.toTaskViewModel | OrgEventDetailPage.jsx | org.workspace.access | ✅ | Get task |
| /api/tasks/{taskId} | PUT | Tasks/Endpoints/UpdateTaskEndpoint.cs.TODO | UpdateTaskRequest | TaskDto | taskService.updateTask | taskAdapter.toTaskViewModel | TaskFormModal.jsx | org.events.manage | ✅ | Update task |
| /api/tasks/{taskId} | DELETE | Tasks/Endpoints/DeleteTaskEndpoint.cs.TODO | None | None | taskService.deleteTask | None | OrgEventDetailPage.jsx | org.events.manage | ✅ | Delete task |
| /api/tasks/{taskId}/status | PUT | Tasks/Endpoints/UpdateTaskStatusEndpoint.cs.TODO | UpdateTaskStatusRequest | TaskDto | taskService.updateTaskStatus | taskAdapter.toTaskViewModel | TaskStatusControl.jsx | org.events.manage | ✅ | Update status |
| /api/tasks/{taskId}/assign | PUT | Tasks/Endpoints/AssignTaskEndpoint.cs.TODO | AssignTaskRequest | TaskDto | taskService.assignTask | taskAdapter.toTaskViewModel | TaskAssignControl.jsx | org.events.manage | ✅ | Assign task |

---

## Requests Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/organizations/{orgId}/requests | GET | Requests/Endpoints/GetOrganizationRequestsEndpoint.cs.TODO | None | RequestDto[] | requestService.getOrganizationRequests | requestAdapter.toRequestViewModel | OrgRequestsPage.jsx | org.requests.view | ✅ | List requests |
| /api/organizations/{orgId}/requests | POST | Requests/Endpoints/CreateOrganizationRequestEndpoint.cs.TODO | CreateRequestRequest | RequestDto | requestService.createOrganizationRequest | requestAdapter.toRequestViewModel | Multiple | JWT | ✅ | Create request |
| /api/requests/{requestId} | GET | Requests/Endpoints/GetRequestByIdEndpoint.cs.TODO | None | RequestDto | requestService.getRequestById | requestAdapter.toRequestViewModel | OrgRequestsPage.jsx | org.requests.view | ✅ | Get request |
| /api/organizations/requests/{requestId}/review | POST | Requests/Endpoints/ReviewRequestEndpoint.cs.TODO | ReviewRequestRequest | RequestDto | requestService.reviewRequest | requestAdapter.toRequestViewModel | OrgRequestsPage.jsx | org.requests.review, org.requests.approve | ✅ | Review request |

---

## Notifications Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/notifications | GET | Notifications/Endpoints/GetNotificationsEndpoint.cs.TODO | None | NotificationDto[] | notificationService.getNotifications | notificationAdapter.toNotificationViewModel | OrgNotificationsPage.jsx | JWT | ✅ | List notifications |
| /api/notifications/unread-count | GET | Notifications/Endpoints/GetUnreadCountEndpoint.cs.TODO | None | UnreadCountResponse | notificationService.getUnreadCount | None | NotificationBadge.jsx | JWT | ✅ | Unread count |
| /api/notifications/{id}/read | POST | Notifications/Endpoints/MarkNotificationReadEndpoint.cs.TODO | None | MarkNotificationReadResponse | notificationService.markNotificationRead | None | OrgNotificationsPage.jsx | JWT | ✅ | Mark read |
| /api/notifications/read-all | POST | Notifications/Endpoints/MarkAllNotificationsReadEndpoint.cs.TODO | None | None | notificationService.markAllNotificationsRead | None | OrgNotificationsPage.jsx | JWT | ✅ | Mark all read |

---

## Friends Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/friends | GET | Friends/Endpoints/GetFriendsEndpoint.cs.TODO | None | FriendDto[] | friendService.getFriends | friendAdapter.toFriendViewModel | UserFriendsPage.jsx | JWT | ✅ | List friends |
| /api/friends/requests | GET | Friends/Endpoints/GetFriendRequestsEndpoint.cs.TODO | None | FriendRequestDto[] | friendService.getFriendRequests | friendAdapter.toFriendRequestViewModel | UserFriendsPage.jsx | JWT | ✅ | List friend requests |
| /api/friends/requests | POST | Friends/Endpoints/SendFriendRequestEndpoint.cs.TODO | SendFriendRequestRequest | FriendRequestDto | friendService.sendFriendRequest | friendAdapter.toFriendRequestViewModel | UserFriendsPage.jsx | JWT | ✅ | Send friend request |
| /api/friends/requests/{id}/accept | POST | Friends/Endpoints/AcceptFriendRequestEndpoint.cs.TODO | None | FriendDto | friendService.acceptFriendRequest | friendAdapter.toFriendViewModel | UserFriendsPage.jsx | JWT | ✅ | Accept request |
| /api/friends/requests/{id}/reject | POST | Friends/Endpoints/RejectFriendRequestEndpoint.cs.TODO | None | None | friendService.rejectFriendRequest | None | UserFriendsPage.jsx | JWT | ✅ | Reject request |

---

## Discover Module

| Backend Route | Method | Backend Skeleton | Request Contract | Response Contract | Frontend Service | Adapter | Page/Component | Permission | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| /api/discover/organizations | GET | Discover/Endpoints/DiscoverOrganizationsEndpoint.cs.TODO | None | DiscoverOrganizationDto[] | discoverService.discoverOrganizations | discoverAdapter.toDiscoverOrganizationViewModel | UserDiscoverPage.jsx | JWT | ✅ | Discover orgs |
| /api/discover/events | GET | Discover/Endpoints/DiscoverEventsEndpoint.cs.TODO | None | DiscoverEventDto[] | discoverService.discoverEvents | discoverAdapter.toDiscoverEventViewModel | UserDiscoverPage.jsx | JWT | ✅ | Discover events |

---

## Explicitly Not Implemented Routes

### No /org/tasks Aggregate Route
- ❌ No `GET /api/organizations/{orgId}/tasks` endpoint
- ❌ No `GET /api/tasks` aggregate endpoint
- ❌ No list-by-org task service
- ❌ No aggregate task board API
- **Reason**: Task CRUD is CORE inside EventDetail tree only; `/org/tasks` page is PROTOTYPE_ONLY placeholder

### No List-by-Category Task Endpoint
- ❌ No `GET /api/categories/{categoryId}/tasks` endpoint
- **Reason**: CategoryDto may include optional `tasks[]` array; if absent, frontend initializes `tasks: []`; no separate list endpoint needed

### No EventMembers Routes (DB Foundation Only)
- ❌ No EventMembers CRUD endpoints
- **Reason**: DB_FOUNDATION_ONLY; no working UI/API in base prototype

### No Attendees Routes (DB Foundation Only)
- ❌ No Attendees CRUD endpoints
- **Reason**: DB_FOUNDATION_ONLY; no working UI/API in base prototype

### No EventRatings Routes (DB Foundation Only)
- ❌ No EventRatings CRUD endpoints
- **Reason**: DB_FOUNDATION_ONLY; no working UI/API in base prototype

### No EventReports Working Routes (DB Foundation Only)
- ❌ No EventReports CRUD endpoints beyond placeholder
- **Reason**: DB_FOUNDATION_ONLY; Reports page is PROTOTYPE_ONLY placeholder

### No Resources Working Routes (DB Foundation Only)
- ❌ No Resources CRUD endpoints beyond placeholder
- **Reason**: DB_FOUNDATION_ONLY; Resources page is PROTOTYPE_ONLY placeholder

### No ActivityHistory Routes (DB Foundation Only)
- ❌ No ActivityHistory CRUD endpoints
- **Reason**: DB_FOUNDATION_ONLY; no working UI/API in base prototype

### No Posts Routes (Hard-Excluded)
- ❌ No Posts CRUD endpoints
- **Reason**: Hard-excluded from rescue v1

### No Comments Routes (Hard-Excluded)
- ❌ No Comments CRUD endpoints
- **Reason**: Hard-excluded from rescue v1

### No Messages/Chat Routes (Placeholder Only)
- ❌ No Messages/Chat CRUD endpoints
- **Reason**: Placeholder only; no working module in base prototype

### No Finance Routes (Placeholder Only)
- ❌ No Finance CRUD endpoints
- **Reason**: Finance-specific module excluded; Finance page is PROTOTYPE_ONLY placeholder

---

## Summary Statistics

### Total Routes Documented
- **Auth**: 3 routes
- **Users**: 6 routes
- **Organizations**: 6 routes
- **Members**: 4 routes
- **RolesPermissions**: 7 routes
- **Departments**: 5 routes
- **Events**: 7 routes
- **Milestones**: 5 routes
- **EventCategories**: 5 routes
- **Tasks**: 6 routes
- **Requests**: 4 routes
- **Notifications**: 4 routes
- **Friends**: 5 routes
- **Discover**: 2 routes

**Total**: 69 routes

### Routes by Permission Type
- **Public**: 4 routes (login, register, public events, public event detail, public org overview)
- **JWT only**: 15 routes (user profile, notifications, friends, discover)
- **org.workspace.access**: 15 routes (list members, list departments, list events, get event, list milestones, list categories, get task)
- **org.overview.write**: 1 route (update org)
- **org.members.manage**: 3 routes (add member, update member dept, remove member)
- **org.roles.view**: 2 routes (list permissions, list roles)
- **org.roles.create/update/delete/assign**: 4 routes (role CRUD, assign role)
- **org.departments.manage**: 3 routes (create/update/delete dept)
- **org.events.create**: 1 route (create event)
- **org.events.manage**: 13 routes (update/delete event, milestone CRUD, category CRUD, task CRUD/status/assign)
- **org.requests.view**: 2 routes (list requests, get request)
- **org.requests.review/approve**: 1 route (review request)

---

## Cross-Layer Consistency Verification

✅ All CORE modules (12) have complete route → contract → service → adapter → page mapping  
✅ All SUPPORTING modules (2) have complete route → contract → service → adapter → page mapping  
✅ All permissions use canonical keys (no non-canonical view permissions)  
✅ No invented routes outside approved list  
✅ Task module: CORE inside EventDetail tree, no aggregate board endpoint  
✅ CategoryDto: may include optional tasks[] array, no separate list endpoint  
✅ getMyOrganizations: belongs to userService, not organizationService  
✅ assignRoleToMember: belongs to roleService, not memberService  
✅ All DB_FOUNDATION_ONLY modules: no working endpoints  
✅ All EXCLUDED modules: no routes created  

---

**End of API_CONTRACT_TODO_MAP.md**
